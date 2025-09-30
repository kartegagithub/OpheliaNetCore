using Azure.AI.OpenAI;
using OpenAI.Chat;
using Ophelia.AI.Interfaces;
using Ophelia.AI.Models;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ophelia.AI.ChatServices
{
    public class AzureOpenAIChatService : BaseChatService
    {
        private readonly AzureOpenAIClient _openAIClient;
        private readonly ChatClient _chatClient;

        public AzureOpenAIChatService(
            AIConfig configuration,
            IChatHistoryStore chatHistoryStore) : base(configuration, chatHistoryStore)
        {
            var endpoint = configuration.LLMConfig.Endpoint ?? throw new InvalidOperationException("Azure OpenAI endpoint not configured");
            var apiKey = configuration.LLMConfig.APIKey ?? throw new InvalidOperationException("Azure OpenAI API key not configured");

            _openAIClient = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));
            _chatClient = _openAIClient.GetChatClient(configuration.LLMConfig.Model);
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

                // Modern OpenAI SDK kullanımı
                var response = await _chatClient.CompleteChatAsync(messages);

                var responseMessage = response.Value.Content[0].Text;

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
                    TokensUsed = response.Value.Usage?.TotalTokenCount ?? 0,
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

                // Modern streaming API kullanımı
                await foreach (var chatUpdate in _chatClient.CompleteChatStreamingAsync(messages))
                {
                    foreach (var contentPart in chatUpdate.ContentUpdate)
                    {
                        if (contentPart.Text != null)
                        {
                            responseBuilder.Append(contentPart.Text);
                            outputAction("message", contentPart.Text);
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