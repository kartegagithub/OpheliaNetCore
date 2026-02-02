using Ophelia.AI.Interfaces;
using Ophelia.AI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
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

        public override async Task<ChatResponse> CompleteChatAsync(string userMessage, string? userId = null, Dictionary<string, string>? filter = null)
        {
            var startTime = DateTime.UtcNow;
            var conversationId = userId ?? Guid.NewGuid().ToString();

            try
            {
                var (chunks, history) = await PrepareContextAsync(userMessage, conversationId, filter);
                var context = BuildContext(chunks);
                var sources = chunks.Select(c => c.Source).Distinct().ToList();

                var model = !string.IsNullOrEmpty(this.Config.LLMConfig.Model) ? this.Config.LLMConfig.Model : "gemini-1.5-pro-latest";
                var requestBody = BuildGeminiRequest(context, userMessage, history);

                var baseUrl = !string.IsNullOrEmpty(this.Config.LLMConfig.Endpoint) 
                    ? this.Config.LLMConfig.Endpoint.TrimEnd('/') 
                    : "https://generativelanguage.googleapis.com/v1beta";

                var url = $"{baseUrl}/models/{model}:generateContent?key={_apiKey}";
                var jsonContent = new StringContent(requestBody.ToJson(), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, jsonContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Gemini API error: {responseContent}");
                }

                var geminiResponse = responseContent.FromJson<GeminiResponse>();
                var responseMessage = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ?? "Yanıt alınamadı";

                if (this.ChatHistoryStore != null)
                {
                    await this.ChatHistoryStore.SaveMessageAsync(conversationId, "user", userMessage);
                    await this.ChatHistoryStore.SaveMessageAsync(conversationId, "assistant", responseMessage);
                }

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

        public override async Task CompleteChatStreamingAsync(string userMessage, Action<string, string> outputAction, string? userId = null, Dictionary<string, string>? filter = null)
        {
            var conversationId = userId ?? Guid.NewGuid().ToString();
            
            try
            {
                var (chunks, history) = await PrepareContextAsync(userMessage, conversationId, filter);
                var context = BuildContext(chunks);
                var sources = chunks.Select(c => c.Source).Distinct().ToList();

                outputAction("sources", sources.ToJson());

                var model = this.Config.LLMConfig.Model ?? "gemini-1.5-pro-latest";
                var requestBody = BuildGeminiRequest(context, userMessage, history);

                var baseUrl = !string.IsNullOrEmpty(this.Config.LLMConfig.Endpoint) 
                    ? this.Config.LLMConfig.Endpoint.TrimEnd('/') 
                    : "https://generativelanguage.googleapis.com/v1";

                var url = $"{baseUrl}/models/{model}:streamGenerateContent?key={_apiKey}";
                var jsonContent = new StringContent(requestBody.ToJson(), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    outputAction("error", $"Gemini API error: {response.StatusCode}");
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
                                var streamResponse = jsonData.FromJson<GeminiResponse>();
                                var text = streamResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

                                if (!string.IsNullOrEmpty(text))
                                {
                                    responseBuilder.Append(text);
                                    outputAction("message", text);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }
                        }
                    }
                }

                if(this.ChatHistoryStore != null)
                {
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

        private object BuildGeminiRequest(string context, string userMessage, List<ChatHistoryMessage> history)
        {
            var systemInstruction = GetSystemPrompt(context);
            var contents = new List<object>();

            // If we have system instruction, treat it as part of the context of the interaction.
            // Since some API versions/models don't support "system_instruction", 
            // we prepend it to the first user message or handle it logicially.
            
            var effectiveHistory = history.TakeLast(this.Config.MaxChatHistoryMessages).ToList();
            
            // Logic: 
            // 1. If history exists, we prepend system instruction to the FIRST history message if it is from user.
            // 2. If first is model, we might need to insert a user message or prepend to current user message.
            // Simplest robust strategy: Prepend to the LATEST user message (current request) if history is long, 
            // OR prepend to first message if it's the start.
            // Actually, "Context" usually implies global context. 
            // Let's prepend to the very first message sent in this batch.
            
            bool systemPromptAdded = false;

            if (effectiveHistory.Any())
            {
                for (int i = 0; i < effectiveHistory.Count; i++)
                {
                    var msg = effectiveHistory[i];
                    var role = msg.Role == "user" ? "user" : "model";
                    var text = msg.Content;

                    if (!systemPromptAdded && role == "user")
                    {
                        // Add system prompt to the first user message found
                        text = $"System Instruction: {systemInstruction}\n\n{text}";
                        systemPromptAdded = true;
                    }

                    contents.Add(new
                    {
                        role = role,
                        parts = new[] { new { text = text } }
                    });
                }
            }

            // Current user message
            var finalUserText = userMessage;
            if (!systemPromptAdded)
            {
                // If we haven't added it (empty history, or history started with model?), add to current message
                finalUserText = $"System Instruction: {systemInstruction}\n\n{userMessage}";
            }

            contents.Add(new
            {
                role = "user",
                parts = new[] { new { text = finalUserText } }
            });

            return new
            {
                // system_instruction removed to avoid 400 Bad Request
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
        public List<GeminiCandidate> Candidates { get; set; } = new List<GeminiCandidate>();
        public GeminiUsageMetadata UsageMetadata { get; set; } = new GeminiUsageMetadata();
    }

    public class GeminiCandidate
    {
        public GeminiContent Content { get; set; } = new GeminiContent();
        public string FinishReason { get; set; } = "";
        public int Index { get; set; }
    }

    public class GeminiContent
    {
        public List<GeminiPart> Parts { get; set; } = new List<GeminiPart>();
        public string Role { get; set; } = "";
    }

    public class GeminiPart
    {
        public string Text { get; set; } = "";
    }

    public class GeminiUsageMetadata
    {
        public int TotalTokenCount { get; set; }
        public int PromptTokenCount { get; set; }
        public int CandidatesTokenCount { get; set; }
        public int ThoughtsTokenCount { get; set; }
    }

    // Factory Pattern for Service Selection
    public interface IChatServiceFactory
    {
        IChatService CreateChatService(string provider);
    }
}
