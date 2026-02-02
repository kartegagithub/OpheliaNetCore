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
    /// <summary>
    /// Z.ai Chat API entegrasyonu (OpenAI uyumlu /v1/chat/completions endpoint)
    /// </summary>
    public class ZaiChatService : BaseChatService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;
        private const string BaseUrl = "https://api.z.ai/api/paas/v4/chat/completions";

        public ZaiChatService(AIConfig configuration, IChatHistoryStore chatHistoryStore)
            : base(configuration, chatHistoryStore)
        {
            _apiKey = configuration.LLMConfig.APIKey ?? throw new InvalidOperationException("Z.ai API key not configured");
            _model = configuration.LLMConfig.Model ?? "zai-1.5-pro";

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        public override async Task<ChatResponse> CompleteChatAsync(string userMessage, string? userId = null, Dictionary<string, string>? filter = null)
        {
            var startTime = DateTime.UtcNow;
            var conversationId = userId ?? Guid.NewGuid().ToString();

            var (chunks, history) = await PrepareContextAsync(userMessage, conversationId, filter);
            var context = this.BuildContext(chunks);
            var sources = chunks.Select(c => c.Source).Distinct().ToList();

            var messages = BuildZaiMessages(userMessage, history);
            var systemPrompt = GetSystemPrompt(context);

            var requestBody = new
            {
                model = _model,
                messages = new List<object>
                {
                    new { role = "system", content = systemPrompt }
                }.Concat(messages).ToList(),
                max_tokens = 4096,
                temperature = 0.7
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(BaseUrl, content);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Z.ai API error: {response.StatusCode} - {responseJson}");

            var responseData = JsonSerializer.Deserialize<JsonElement>(responseJson);
            var message = responseData.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

            if (this.ChatHistoryStore != null)
            {
                await this.ChatHistoryStore.SaveMessageAsync(conversationId, "user", userMessage);
                await this.ChatHistoryStore.SaveMessageAsync(conversationId, "assistant", message);
            }

            var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

            return new ChatResponse
            {
                Message = message,
                Sources = sources,
                TokensUsed = responseData.GetProperty("usage").GetProperty("total_tokens").GetInt32(),
                ProcessingTimeMs = processingTime,
                ConversationId = conversationId
            };
        }

        public override async Task CompleteChatStreamingAsync(string userMessage, Action<string, string> outputAction, string? userId = null, Dictionary<string, string>? filter = null)
        {
            var conversationId = userId ?? Guid.NewGuid().ToString();

            var (chunks, history) = await PrepareContextAsync(userMessage, conversationId, filter);
            var context = this.BuildContext(chunks);
            var sources = chunks.Select(c => c.Source).Distinct().ToList();
            outputAction("sources", JsonSerializer.Serialize(sources));

            var systemPrompt = GetSystemPrompt(context);
            var messages = BuildZaiMessages(userMessage, history);

            var requestBody = new
            {
                model = _model,
                messages = new List<object>
                {
                    new { role = "system", content = systemPrompt }
                }.Concat(messages).ToList(),
                stream = true
            };

            var requestJson = JsonSerializer.Serialize(requestBody);
            var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl)
            {
                Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");

            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            var stream = await response.Content.ReadAsStreamAsync();
            var reader = new StreamReader(stream);

            var fullResponse = new StringBuilder();

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (line.StartsWith("data: "))
                {
                    var jsonPart = line.Substring(6);
                    if (jsonPart == "[DONE]") break;

                    var data = JsonSerializer.Deserialize<JsonElement>(jsonPart);
                    var delta = data.GetProperty("choices")[0].GetProperty("delta");
                    if (delta.TryGetProperty("content", out var chunk))
                    {
                        var text = chunk.GetString();
                        if (!string.IsNullOrEmpty(text))
                        {
                            outputAction("message", text);
                            fullResponse.Append(text);
                        }
                    }
                }
            }

            if (this.ChatHistoryStore != null)
            {
                await this.ChatHistoryStore.SaveMessageAsync(conversationId, "user", userMessage);
                await this.ChatHistoryStore.SaveMessageAsync(conversationId, "assistant", fullResponse.ToString());
            }

            outputAction("done", "");
        }

        private List<object> BuildZaiMessages(string userMessage, List<ChatHistoryMessage> history)
        {
            var messages = new List<object>();
            foreach (var msg in history.TakeLast(this.Config.MaxChatHistoryMessages))
            {
                messages.Add(new
                {
                    role = msg.Role,
                    content = msg.Content
                });
            }

            messages.Add(new
            {
                role = "user",
                content = userMessage
            });

            return messages;
        }
    }
}
