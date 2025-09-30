using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using Ophelia.AI.Interfaces;
using Ophelia.AI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Message = Anthropic.SDK.Messaging.Message;

namespace Ophelia.AI.ChatServices
{
    public class ClaudeChatService : BaseChatService
    {
        private readonly AnthropicClient _claudeClient;

        public ClaudeChatService(
            AIConfig configuration,
            IChatHistoryStore chatHistoryStore) : base(configuration, chatHistoryStore)
        {
            var apiKey = configuration.LLMConfig.APIKey ?? throw new InvalidOperationException("Claude API key not configured");

            _claudeClient = new AnthropicClient(apiKey);
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

                var messages = BuildClaudeMessages(userMessage, history);
                var systemPrompt = GetSystemPrompt(context);

                var model = this.Config.LLMConfig.Model ?? "claude-3-sonnet-20240229";

                var messageRequest = new MessageParameters
                {
                    Model = model,
                    MaxTokens = 4096,
                    Messages = messages,
                    System = (new SystemMessage[] { new SystemMessage(systemPrompt) }).ToList(),
                };

                var response = await _claudeClient.Messages.GetClaudeMessageAsync(messageRequest);
                var responseMessage = string.Join("", response.Content);//TODO: To be validated

                if(this.ChatHistoryStore != null)
                {
                    await this.ChatHistoryStore.SaveMessageAsync(conversationId, "user", userMessage);
                    await this.ChatHistoryStore.SaveMessageAsync(conversationId, "assistant", responseMessage);
                }
                
                var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

                return new ChatResponse
                {
                    Message = responseMessage,
                    Sources = sources,
                    TokensUsed = response.Usage.InputTokens + response.Usage.OutputTokens,
                    ProcessingTimeMs = processingTime,
                    ConversationId = conversationId
                };
            }
            catch (Exception)
            {
                throw;
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

                var messages = BuildClaudeMessages(userMessage, history);
                var systemPrompt = GetSystemPrompt(context);
                var model = this.Config.LLMConfig.Model ?? "claude-3-sonnet-20240229";

                var messageRequest = new MessageParameters
                {
                    Model = model,
                    MaxTokens = 4096,
                    Messages = messages,
                    System = (new SystemMessage[] { new SystemMessage(systemPrompt) }).ToList(),
                    Stream = true
                };

                var responseBuilder = new StringBuilder();

                await foreach (var streamEvent in _claudeClient.Messages.StreamClaudeMessageAsync(messageRequest))
                {
                    if (streamEvent.Delta?.Text != null)
                    {
                        responseBuilder.Append(streamEvent.Delta.Text);
                        outputAction("message", streamEvent.Delta.Text);
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

        private List<Message> BuildClaudeMessages(string userMessage, List<ChatHistoryMessage> history)
        {
            var messages = new List<Message>();

            foreach (var historyMsg in history.TakeLast(this.Config.MaxChatHistoryMessages))
            {
                messages.Add(new Message
                {
                    Role = historyMsg.Role == "user" ? RoleType.User : RoleType.Assistant,
                    Content = new List<ContentBase> { new TextContent { Text = historyMsg.Content } }
                });
            }

            messages.Add(new Message
            {
                Role = RoleType.User,
                Content = new List<ContentBase> { new TextContent { Text = userMessage } }
            });

            return messages;
        }
    }
}
