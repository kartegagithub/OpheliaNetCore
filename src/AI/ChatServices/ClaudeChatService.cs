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

        public override async Task<ChatResponse> CompleteChatAsync(string userMessage, string? userId = null, Dictionary<string, string>? filter = null, List<ChatAttachment>? attachments = null)
        {
            var startTime = DateTime.UtcNow;
            var conversationId = userId ?? Guid.NewGuid().ToString();

            try
            {
                var (chunks, history) = await PrepareContextAsync(userMessage, conversationId, filter);
                var context = BuildContext(chunks);
                var sources = chunks.Select(c => c.Source).Distinct().ToList();

                var messages = BuildClaudeMessages(userMessage, history, attachments);
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
                    await this.ChatHistoryStore.SaveMessageAsync(conversationId, "user", userMessage, attachments);
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

        public override async Task CompleteChatStreamingAsync(string userMessage, Action<string, string> outputAction, string? userId = null, Dictionary<string, string>? filter = null, List<ChatAttachment>? attachments = null)
        {
            var conversationId = userId ?? Guid.NewGuid().ToString();
            try
            {
                var (chunks, history) = await PrepareContextAsync(userMessage, conversationId, filter);
                var context = BuildContext(chunks);
                var sources = chunks.Select(c => c.Source).Distinct().ToList();

                outputAction("sources", JsonSerializer.Serialize(sources));

                var messages = BuildClaudeMessages(userMessage, history, attachments);
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
                    await this.ChatHistoryStore.SaveMessageAsync(conversationId, "user", userMessage, attachments);
                    await this.ChatHistoryStore.SaveMessageAsync(conversationId, "assistant", responseBuilder.ToString());
                }

                outputAction("done", "");
            }
            catch (Exception ex)
            {
                outputAction("error", ex.Message);
            }
        }

        private List<Message> BuildClaudeMessages(string userMessage, List<ChatHistoryMessage> history, List<ChatAttachment>? attachments)
        {
            var messages = new List<Message>();

            foreach (var historyMsg in history.TakeLast(this.Config.MaxChatHistoryMessages))
            {
                if (historyMsg.Role == "user")
                {
                    messages.Add(new Message
                    {
                        Role = RoleType.User,
                        Content = BuildUserContent(historyMsg.Content, historyMsg.Attachments)
                    });
                }
                else
                {
                    messages.Add(new Message
                    {
                        Role = RoleType.Assistant,
                        Content = new List<ContentBase> { new TextContent { Text = historyMsg.Content } }
                    });
                }
            }

            messages.Add(new Message
            {
                Role = RoleType.User,
                Content = BuildUserContent(userMessage, attachments)
            });

            return messages;
        }

        private List<ContentBase> BuildUserContent(string userMessage, List<ChatAttachment>? attachments)
        {
            var contentItems = new List<ContentBase>();
            var unsupported = new List<ChatAttachment>();

            if (attachments != null)
            {
                foreach (var attachment in attachments)
                {
                    var mimeType = GetAttachmentMimeType(attachment);
                    var fileName = GetAttachmentFileName(attachment);
                    var base64 = GetAttachmentBase64(attachment);

                    if (!string.IsNullOrWhiteSpace(attachment.FileUrl))
                    {
                        if (IsImageMimeType(mimeType))
                        {
                            contentItems.Add(new ImageContent
                            {
                                Source = new ImageSource
                                {
                                    Type = SourceType.url,
                                    Url = attachment.FileUrl,
                                    MediaType = mimeType
                                }
                            });
                        }
                        else
                        {
                            contentItems.Add(new DocumentContent
                            {
                                Source = new DocumentSource
                                {
                                    Type = SourceType.url,
                                    Url = attachment.FileUrl,
                                    MediaType = mimeType
                                },
                                Title = fileName
                            });
                        }
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(base64))
                    {
                        if (IsImageMimeType(mimeType))
                        {
                            contentItems.Add(new ImageContent
                            {
                                Source = new ImageSource
                                {
                                    Type = SourceType.base64,
                                    Data = base64,
                                    MediaType = mimeType
                                }
                            });
                        }
                        else
                        {
                            contentItems.Add(new DocumentContent
                            {
                                Source = new DocumentSource
                                {
                                    Type = SourceType.base64,
                                    Data = base64,
                                    MediaType = mimeType
                                },
                                Title = fileName
                            });
                        }
                    }
                    else
                    {
                        unsupported.Add(attachment);
                    }
                }
            }

            contentItems.Add(new TextContent
            {
                Text = AppendAttachmentSummary(userMessage, unsupported)
            });

            return contentItems;
        }
    }
}
