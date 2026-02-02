using Ophelia;
using Ophelia.AI.Interfaces;
using Ophelia.AI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ophelia.AI.ChatServices
{
    public class HuggingFaceChatService : BaseChatService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;

        public HuggingFaceChatService(AIConfig configuration, IChatHistoryStore chatHistoryStore) : base(configuration, chatHistoryStore)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", configuration.LLMConfig.APIKey);

            _apiUrl = $"{configuration.LLMConfig.Endpoint ?? "https://api-inference.huggingface.co"}/models/{configuration.LLMConfig.Model}";
        }

        public override async Task<ChatResponse> CompleteChatAsync(string userMessage, string? userId = null, Dictionary<string, string>? filter = null)
        {
            var startTime = DateTime.UtcNow;
            var conversationId = userId ?? Guid.NewGuid().ToString();

            try
            {
                var contextData = await this.PrepareContextAsync(userMessage, userId);

                // Context oluştur
                var context = BuildContext(contextData.chunks);
                var sources = contextData.chunks.Select(c => c.Source).Distinct().ToList();

                // Prompt oluştur
                var prompt = BuildPrompt(context, userMessage, contextData.history);

                // Hugging Face API request
                var requestBody = new
                {
                    inputs = prompt,
                    parameters = new
                    {
                        max_new_tokens = this.Config.LLMConfig.MaxTokens,
                        temperature = this.Config.LLMConfig.Temperature,
                        top_p = this.Config.LLMConfig.TopP,
                        return_full_text = false
                    }
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(_apiUrl, content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<List<HuggingFaceResponse>>(responseBody);
                var responseMessage = result?[0]?.generated_text ?? string.Empty;

                if (this.ChatHistoryStore != null)
                {
                    // Chat geçmişini kaydet
                    await this.ChatHistoryStore.SaveMessageAsync(conversationId, "user", userMessage);
                    await this.ChatHistoryStore.SaveMessageAsync(conversationId, "assistant", responseMessage);
                }

                var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

                return new ChatResponse
                {
                    Message = responseMessage,
                    Sources = sources,
                    TokensUsed = EstimateTokens(prompt + responseMessage),
                    ProcessingTimeMs = processingTime,
                    ConversationId = conversationId
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override async Task CompleteChatStreamingAsync(string userMessage, Action<string, string> outputAction, string? userId = null, Dictionary<string, string>? filter = null)
        {
            var conversationId = userId ?? Guid.NewGuid().ToString();

            try
            {
                // Context oluştur
                var contextData = await this.PrepareContextAsync(userMessage, userId);
                var context = BuildContext(contextData.chunks);

                // Kaynakları gönder
                var sources = contextData.chunks.Select(c => c.Source).Distinct().ToList();
                outputAction("sources", JsonSerializer.Serialize(sources));

                // Prompt oluştur
                var prompt = BuildPrompt(context, userMessage, contextData.history);

                // Hugging Face Streaming API request
                var requestBody = new
                {
                    inputs = prompt,
                    parameters = new
                    {
                        max_new_tokens = this.Config.LLMConfig.MaxTokens,
                        temperature = this.Config.LLMConfig.Temperature,
                        top_p = this.Config.LLMConfig.TopP,
                        return_full_text = false
                    },
                    stream = true
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                var request = new HttpRequestMessage(HttpMethod.Post, _apiUrl)
                {
                    Content = content
                };

                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                var responseBuilder = new StringBuilder();

                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var reader = new StreamReader(stream))
                {
                    string? line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: "))
                            continue;

                        var jsonData = line.Substring(6); // "data: " kısmını atla

                        if (jsonData == "[DONE]")
                            break;

                        try
                        {
                            var streamResponse = JsonSerializer.Deserialize<HuggingFaceStreamResponse>(jsonData);
                            var text = streamResponse?.token?.text;

                            if (!string.IsNullOrEmpty(text))
                            {
                                responseBuilder.Append(text);
                                outputAction("message", text);
                            }
                        }
                        catch
                        {
                            // JSON parse hatalarını yoksay
                            continue;
                        }
                    }
                }

                if (this.ChatHistoryStore != null)
                {
                    // Chat geçmişini kaydet
                    await this.ChatHistoryStore.SaveMessageAsync(conversationId, "user", userMessage);
                    await this.ChatHistoryStore.SaveMessageAsync(conversationId, "assistant", responseBuilder.ToString());
                }

                outputAction("done", "");
            }
            catch (Exception ex)
            {
                outputAction("error", ex.Message);
            }
        }

        private string BuildPrompt(string context, string userMessage, List<ChatHistoryMessage> history)
        {
            var promptBuilder = new StringBuilder();

            // System prompt
            var systemPrompt = this.Config.LLMConfig.SystemPrompt.Replace("{context}", context);
            promptBuilder.AppendLine($"<|system|>\n{systemPrompt}\n<|end|>");

            // Chat geçmişi
            foreach (var historyMsg in history.TakeLast(this.Config.MaxChatHistoryMessages))
            {
                if (historyMsg.Role == "user")
                    promptBuilder.AppendLine($"<|user|>\n{historyMsg.Content}\n<|end|>");
                else
                    promptBuilder.AppendLine($"<|assistant|>\n{historyMsg.Content}\n<|end|>");
            }

            // Mevcut kullanıcı mesajı
            promptBuilder.AppendLine($"<|user|>\n{userMessage}\n<|end|>");
            promptBuilder.Append("<|assistant|>");

            return promptBuilder.ToString();
        }

        private static int EstimateTokens(string text)
        {
            // Basit token tahmini (ortalama 4 karakter = 1 token)
            return text.Length / 4;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "<Pending>")]
        public override void Dispose()
        {
            _httpClient?.Dispose();
            base.Dispose();
        }

        // Hugging Face API Response Models
        private class HuggingFaceResponse
        {
            public string generated_text { get; set; } = string.Empty;
        }

        private class HuggingFaceStreamResponse
        {
            public TokenInfo? token { get; set; }
        }

        private class TokenInfo
        {
            public string text { get; set; } = string.Empty;
        }
    }
}