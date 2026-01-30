using OpenAI.Chat;
using OpenAI;
using Ophelia.AI.Interfaces;
using Ophelia.AI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ophelia.AI.ChatServices
{
    public class OpenAICompatibleChatService : BaseChatService
    {
        private readonly ChatClient _chatClient;

        public OpenAICompatibleChatService(AIConfig configuration, IChatHistoryStore chatHistoryStore, string defaultEndpoint = null) : base(configuration, chatHistoryStore)
        {
            var apiKey = configuration.LLMConfig.APIKey;
            // Some local services like Ollama/LMStudio might not require an API key, so allow empty/dummy if configured
            if (string.IsNullOrEmpty(apiKey)) apiKey = "dummy-key";

            var endpoint = configuration.LLMConfig.Endpoint ?? defaultEndpoint;
            
            var options = new OpenAIClientOptions();
            if (!string.IsNullOrEmpty(endpoint))
            {
                options.Endpoint = new Uri(endpoint);
            }
            
            // Handle models that might need specific client configurations
            var model = configuration.LLMConfig.Model;
            if (string.IsNullOrEmpty(model))
            {
                // Default models
                if (configuration.LLMConfig.Type == LLMType.Groq) model = "llama3-70b-8192";
                else if (configuration.LLMConfig.Type == LLMType.DeepSeek) model = "deepseek-chat";
                else if (configuration.LLMConfig.Type == LLMType.Ollama) model = "llama3";
                else if (configuration.LLMConfig.Type == LLMType.LMStudio) model = "local-model";
            }

            // Provide a resilient client creation
            // Note: If using direct ChatClient constructor, ensure options.Endpoint is respected.
            // Using OpenAIClient factory is safer for custom endpoints in some SDK versions.
            var client = new OpenAIClient(new System.ClientModel.ApiKeyCredential(apiKey), options);
            _chatClient = client.GetChatClient(model);
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

                var messages = BuildChatMessages(context, userMessage, history);

                var chatCompletion = await _chatClient.CompleteChatAsync(messages);
                var responseMessage = chatCompletion.Value.Content[0].Text;

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
                    TokensUsed = chatCompletion.Value.Usage.TotalTokenCount,
                    ProcessingTimeMs = processingTime,
                    ConversationId = conversationId
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Chat processing failed: {ex.Message}", ex);
            }
        }

        public override async Task CompleteChatStreamingAsync(string userMessage, Action<string, string> outputAction, string? userId = null)
        {
            var conversationId = userId ?? Guid.NewGuid().ToString();
            try
            {
                var (chunks, history) = await PrepareContextAsync(userMessage, conversationId);
                var context = BuildContext(chunks);
                var sources = chunks.Select(c => c.Source).Distinct().ToList();

                outputAction("sources", JsonSerializer.Serialize(sources));

                var messages = BuildChatMessages(context, userMessage, history);
                var responseBuilder = new StringBuilder();

                await foreach (var update in _chatClient.CompleteChatStreamingAsync(messages))
                {
                    foreach (var contentPart in update.ContentUpdate)
                    {
                        var text = contentPart.Text;
                        if (!string.IsNullOrEmpty(text))
                        {
                            responseBuilder.Append(text);
                            outputAction("message", text);
                        }
                    }
                }

                if (this.ChatHistoryStore != null)
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

        private List<ChatMessage> BuildChatMessages(string context, string userMessage, List<ChatHistoryMessage> history)
        {
            var messages = new List<ChatMessage>();

            var systemPrompt = GetSystemPrompt(context);
            messages.Add(ChatMessage.CreateSystemMessage(systemPrompt));

            foreach (var historyMsg in history.TakeLast(this.Config.MaxChatHistoryMessages))
            {
                if (historyMsg.Role == "user")
                    messages.Add(ChatMessage.CreateUserMessage(historyMsg.Content));
                else
                    messages.Add(ChatMessage.CreateAssistantMessage(historyMsg.Content));
            }

            messages.Add(ChatMessage.CreateUserMessage(userMessage));
            return messages;
        }
    }
}
