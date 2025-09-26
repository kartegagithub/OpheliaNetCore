using Microsoft.Extensions.Logging;
using Ophelia.AI.Interfaces;
using Ophelia.AI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ophelia.AI.ChatServices
{
    public class GeminiChatService : BaseChatService
    {
        private HttpClient? _httpClient;
        private string _apiKey;

        public GeminiChatService(
            AIConfig configuration,
            IChatHistoryStore chatHistoryStore) : base(configuration, chatHistoryStore)
        {
            _apiKey = configuration.LLMConfig.APIKey ?? throw new InvalidOperationException("Gemini API key not configured");
            this._httpClient = new HttpClient();
        }

        public override async Task<ChatResponse> ProcessQueryAsync(string userMessage, string? userId = null)
        {
            var startTime = DateTime.UtcNow;
            var conversationId = userId ?? Guid.NewGuid().ToString();

            try
            {
                var (chunks, history) = await PrepareContextAsync(userMessage, conversationId);
                var context = BuildContext(chunks);
                var sources = chunks.Select(c => c.Source).Distinct().ToList();

                var model = _configuration.LLMConfig.Model ?? "gemini-1.5-pro-latest";
                var requestBody = BuildGeminiRequest(context, userMessage, history);

                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_apiKey}";
                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, jsonContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Gemini API error: {response.StatusCode}");
                }

                var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent);
                var responseMessage = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ?? "Yanıt alınamadı";

                await _chatHistoryStore.SaveMessageAsync(conversationId, "user", userMessage);
                await _chatHistoryStore.SaveMessageAsync(conversationId, "assistant", responseMessage);

                var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

                return new ChatResponse
                {
                    Message = responseMessage,
                    Sources = sources,
                    TokensUsed = geminiResponse?.UsageMetadata?.TotalTokenCount ?? 0,
                    ProcessingTimeMs = processingTime,
                    ConversationId = conversationId
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override async Task ProcessQueryStreamAsync(string userMessage, Stream outputStream, string? userId = null)
        {
            var conversationId = userId ?? Guid.NewGuid().ToString();
            var writer = new StreamWriter(outputStream, Encoding.UTF8, leaveOpen: true, bufferSize: 1024);

            try
            {
                var (chunks, history) = await PrepareContextAsync(userMessage, conversationId);
                var context = BuildContext(chunks);
                var sources = chunks.Select(c => c.Source).Distinct().ToList();

                await SendSseEventAsync(writer, "sources", JsonSerializer.Serialize(sources));

                var model = _configuration.LLMConfig.Model ?? "gemini-1.5-pro-latest";
                var requestBody = BuildGeminiRequest(context, userMessage, history);

                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:streamGenerateContent?key={_apiKey}";
                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    await SendSseEventAsync(writer, "error", $"Gemini API error: {response.StatusCode}");
                    return;
                }

                var responseBuilder = new StringBuilder();
                using var stream = await response.Content.ReadAsStreamAsync();
                using var reader = new StreamReader(stream);

                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (line.StartsWith("data: "))
                    {
                        var jsonData = line.Substring(6);
                        if (jsonData != "[DONE]")
                        {
                            try
                            {
                                var streamResponse = JsonSerializer.Deserialize<GeminiResponse>(jsonData);
                                var text = streamResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

                                if (!string.IsNullOrEmpty(text))
                                {
                                    responseBuilder.Append(text);
                                    await SendSseEventAsync(writer, "message", text);
                                }
                            }
                            catch (JsonException ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }
                        }
                    }
                }

                await _chatHistoryStore.SaveMessageAsync(conversationId, "user", userMessage);
                await _chatHistoryStore.SaveMessageAsync(conversationId, "assistant", responseBuilder.ToString());

                await SendSseEventAsync(writer, "done", "");
                await writer.FlushAsync();
            }
            catch (Exception ex)
            {
                await SendSseEventAsync(writer, "error", ex.Message);
                await writer.FlushAsync();
            }
        }

        private object BuildGeminiRequest(string context, string userMessage, List<ChatHistoryMessage> history)
        {
            var systemInstruction = GetSystemPrompt(context);
            var contents = new List<object>();

            // Chat history
            foreach (var historyMsg in history.TakeLast(this._configuration.MaxChatHistoryMessages))
            {
                contents.Add(new
                {
                    role = historyMsg.Role == "user" ? "user" : "model",
                    parts = new[] { new { text = historyMsg.Content } }
                });
            }

            // Current user message
            contents.Add(new
            {
                role = "user",
                parts = new[] { new { text = userMessage } }
            });

            return new
            {
                system_instruction = new
                {
                    parts = new[] { new { text = systemInstruction } }
                },
                contents,
                generationConfig = new
                {
                    temperature = 0.7,
                    topK = 40,
                    topP = 0.95,
                    maxOutputTokens = 8192
                }
            };
        }
    }

    public class GeminiResponse
    {
        public List<GeminiCandidate>? Candidates { get; set; }
        public GeminiUsageMetadata? UsageMetadata { get; set; }
    }

    public class GeminiCandidate
    {
        public GeminiContent? Content { get; set; }
    }

    public class GeminiContent
    {
        public List<GeminiPart>? Parts { get; set; }
    }

    public class GeminiPart
    {
        public string? Text { get; set; }
    }

    public class GeminiUsageMetadata
    {
        public int TotalTokenCount { get; set; }
    }

    // Factory Pattern for Service Selection
    public interface IChatServiceFactory
    {
        IChatService CreateChatService(string provider);
    }
}
