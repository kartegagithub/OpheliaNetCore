using Ophelia.AI.Interfaces;
using Ophelia.AI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ophelia.AI.ChatServices
{
    public class CohereChatService : BaseChatService
    {
        private readonly HttpClient _httpClient;
        private readonly string _model;

        public CohereChatService(AIConfig configuration, IChatHistoryStore chatHistoryStore) : base(configuration, chatHistoryStore)
        {
            var apiKey = configuration.LLMConfig.APIKey ?? throw new InvalidOperationException("API Key is required for Cohere");
            _model = configuration.LLMConfig.Model ?? "command-r-plus";

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }

        public override async Task<ChatResponse> CompleteChatAsync(string userMessage, string? userId = null)
        {
            var startTime = DateTime.UtcNow;
            var conversationId = userId ?? Guid.NewGuid().ToString();

            try
            {
                var (chunks, history) = await PrepareContextAsync(userMessage, conversationId);
                var context = BuildContext(chunks);
                var sources = chunks.Select(c => c.Source).Distinct().ToList();

                var chatHistory = history.TakeLast(this.Config.MaxChatHistoryMessages).Select(h => new 
                {
                    role = h.Role == "user" ? "USER" : "CHATBOT",
                    message = h.Content
                }).ToList();

                var requestBody = new
                {
                    message = userMessage,
                    model = _model,
                    chat_history = chatHistory,
                    preamble = GetSystemPrompt(context)
                };

                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("https://api.cohere.com/v1/chat", content);
                
                response.EnsureSuccessStatusCode();
                var responseString = await response.Content.ReadAsStringAsync();
                
                using var doc = JsonDocument.Parse(responseString);
                string responseMessage = doc.RootElement.GetProperty("text").GetString();

                if (this.ChatHistoryStore != null)
                {
                    await this.ChatHistoryStore.SaveMessageAsync(conversationId, "user", userMessage);
                    await this.ChatHistoryStore.SaveMessageAsync(conversationId, "assistant", responseMessage);
                }

                var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                
                int tokensUsed = 0;
                try 
                { 
                    if (doc.RootElement.TryGetProperty("meta", out var meta) && 
                        meta.TryGetProperty("tokens", out var tokens) && 
                        tokens.TryGetProperty("output_tokens", out var outputTokens))
                    {
                        tokensUsed = outputTokens.GetInt32();
                    }
                } catch { }

                return new ChatResponse
                {
                    Message = responseMessage,
                    Sources = sources,
                    TokensUsed = tokensUsed,
                    ProcessingTimeMs = processingTime,
                    ConversationId = conversationId
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Cohere processing failed: {ex.Message}", ex);
            }
        }

        public override async Task CompleteChatStreamingAsync(string userMessage, Action<string, string> outputAction, string? userId = null)
        {
             // Simple fallback to non-streaming for now
             var response = await CompleteChatAsync(userMessage, userId);
             outputAction("sources", JsonSerializer.Serialize(response.Sources));
             outputAction("message", response.Message);
             outputAction("done", "");
        }
    }
}
