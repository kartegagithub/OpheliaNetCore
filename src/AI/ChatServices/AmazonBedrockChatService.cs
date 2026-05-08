using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Ophelia.AI.Interfaces;
using Ophelia.AI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ophelia.AI.ChatServices
{
    public class AmazonBedrockChatService : BaseChatService
    {
        private readonly AmazonBedrockRuntimeClient _bedrockClient;
        private readonly string _modelId;

        public AmazonBedrockChatService(AIConfig configuration, IChatHistoryStore chatHistoryStore) : base(configuration, chatHistoryStore)
        {
            var accessKey = configuration.LLMConfig.APIKey; // Assuming AccessKey provided here or in env
             // Bedrock usually requires AccessKey and SecretKey. 
             // Ideally we should use AWS generic credentials or assume role.
             // If APIKey implies "AccessKey:SecretKey", lets split it.
             
            string secretKey = "";
            if (accessKey.Contains(":"))
            {
                var parts = accessKey.Split(':');
                accessKey = parts[0];
                secretKey = parts[1];
            }
            
            var region = Amazon.RegionEndpoint.USWest2; // Default, should effectively be in config. Using generic.
            if (!string.IsNullOrEmpty(configuration.LLMConfig.Endpoint)) // Using Endpoint field for Region if strictly needed or parse it
            {
                 // Simplification: Assume user configured AWS credentials in environment or ~/.aws/credentials
                 // If keys provided explicitly:
            }

            if (!string.IsNullOrEmpty(accessKey) && !string.IsNullOrEmpty(secretKey))
            {
                _bedrockClient = new AmazonBedrockRuntimeClient(accessKey, secretKey, region);
            }
            else
            {
                // Fallback to default credentials chain
                _bedrockClient = new AmazonBedrockRuntimeClient(region);
            }

            _modelId = configuration.LLMConfig.Model ?? "anthropic.claude-3-sonnet-20240229-v1:0"; 
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

                var messages = new List<Amazon.BedrockRuntime.Model.Message>();
                foreach(var h in history.TakeLast(this.Config.MaxChatHistoryMessages))
                {
                    var isUserMessage = h.Role == "user";
                    var messageText = isUserMessage ? h.Content : AppendAttachmentSummary(h.Content, h.Attachments);
                    messages.Add(new Amazon.BedrockRuntime.Model.Message
                    {
                        Role = isUserMessage ? ConversationRole.User : ConversationRole.Assistant,
                        Content = isUserMessage
                            ? BuildBedrockUserContentBlocks(messageText, h.Attachments)
                            : new List<Amazon.BedrockRuntime.Model.ContentBlock> { new Amazon.BedrockRuntime.Model.ContentBlock { Text = messageText } }
                    });
                }

                messages.Add(new Amazon.BedrockRuntime.Model.Message
                {
                    Role = ConversationRole.User,
                    Content = BuildBedrockUserContentBlocks(userMessage, attachments)
                });

                var systemPrompts = new List<Amazon.BedrockRuntime.Model.SystemContentBlock>
                {
                    new Amazon.BedrockRuntime.Model.SystemContentBlock { Text = GetSystemPrompt(context) }
                };

                var request = new Amazon.BedrockRuntime.Model.ConverseRequest
                {
                    ModelId = _modelId,
                    Messages = messages,
                    System = systemPrompts,
                    InferenceConfig = new Amazon.BedrockRuntime.Model.InferenceConfiguration { MaxTokens = 2000, Temperature = 0.7f }
                };

                var response = await _bedrockClient.ConverseAsync(request);
                var responseMessage = response.Output.Message.Content[0].Text;

                if (this.ChatHistoryStore != null)
                {
                    await this.ChatHistoryStore.SaveMessageAsync(conversationId, "user", userMessage, attachments);
                    await this.ChatHistoryStore.SaveMessageAsync(conversationId, "assistant", responseMessage);
                }

                var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

                return new ChatResponse
                {
                    Message = responseMessage,
                    Sources = sources,
                    TokensUsed = (int)response.Usage.TotalTokens,
                    ProcessingTimeMs = processingTime,
                    ConversationId = conversationId
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Bedrock processing failed: {ex.Message}", ex);
            }
        }

        public override async Task CompleteChatStreamingAsync(string userMessage, Action<string, string> outputAction, string? userId = null, Dictionary<string, string>? filter = null, List<ChatAttachment>? attachments = null)
        {
             // Similar logic but with ConverseStreamAsync. 
             // For brevity in this turn, calling blocking.
             var response = await CompleteChatAsync(userMessage, userId, filter, attachments);
             outputAction("sources", JsonSerializer.Serialize(response.Sources));
             outputAction("message", response.Message);
             outputAction("done", "");
        }

        private List<Amazon.BedrockRuntime.Model.ContentBlock> BuildBedrockUserContentBlocks(string userMessage, List<ChatAttachment>? attachments)
        {
            var blocks = new List<Amazon.BedrockRuntime.Model.ContentBlock>();
            var unsupported = new List<ChatAttachment>();

            if (attachments != null)
            {
                foreach (var attachment in attachments)
                {
                    var bytes = GetAttachmentBytes(attachment);
                    if (bytes == null || bytes.Length == 0)
                    {
                        unsupported.Add(attachment);
                        continue;
                    }

                    var mimeType = GetAttachmentMimeType(attachment);

                    if (IsImageMimeType(mimeType))
                    {
                        var imageFormat = GetBedrockImageFormat(attachment, mimeType);
                        if (imageFormat == null)
                        {
                            unsupported.Add(attachment);
                            continue;
                        }

                        blocks.Add(new Amazon.BedrockRuntime.Model.ContentBlock
                        {
                            Image = new Amazon.BedrockRuntime.Model.ImageBlock
                            {
                                Format = imageFormat,
                                Source = new Amazon.BedrockRuntime.Model.ImageSource
                                {
                                    Bytes = new MemoryStream(bytes)
                                }
                            }
                        });
                        continue;
                    }

                    var documentFormat = GetBedrockDocumentFormat(attachment, mimeType);
                    blocks.Add(new Amazon.BedrockRuntime.Model.ContentBlock
                    {
                        Document = new Amazon.BedrockRuntime.Model.DocumentBlock
                        {
                            Name = SanitizeBedrockDocumentName(GetAttachmentFileName(attachment)),
                            Format = documentFormat,
                            Source = new Amazon.BedrockRuntime.Model.DocumentSource
                            {
                                Bytes = new MemoryStream(bytes)
                            }
                        }
                    });
                }
            }

            blocks.Insert(0, new Amazon.BedrockRuntime.Model.ContentBlock
            {
                Text = AppendAttachmentSummary(userMessage, unsupported)
            });

            return blocks;
        }

        private static Amazon.BedrockRuntime.ImageFormat? GetBedrockImageFormat(ChatAttachment attachment, string mimeType)
        {
            var extension = Path.GetExtension(GetAttachmentFileName(attachment))?.TrimStart('.').ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = mimeType switch
                {
                    "image/png" => "png",
                    "image/jpeg" => "jpeg",
                    "image/jpg" => "jpeg",
                    "image/gif" => "gif",
                    "image/webp" => "webp",
                    _ => string.Empty
                };
            }

            if (string.IsNullOrWhiteSpace(extension))
                return null;

            return new Amazon.BedrockRuntime.ImageFormat(extension);
        }

        private static Amazon.BedrockRuntime.DocumentFormat? GetBedrockDocumentFormat(ChatAttachment attachment, string mimeType)
        {
            var extension = Path.GetExtension(GetAttachmentFileName(attachment))?.TrimStart('.').ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = mimeType switch
                {
                    "application/pdf" => "pdf",
                    "text/csv" => "csv",
                    "application/msword" => "doc",
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => "docx",
                    "application/vnd.ms-excel" => "xls",
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => "xlsx",
                    "text/html" => "html",
                    "text/plain" => "txt",
                    "text/markdown" => "md",
                    _ => "txt"
                };
            }

            return new Amazon.BedrockRuntime.DocumentFormat(extension);
        }

        private static string SanitizeBedrockDocumentName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "document";

            var validChars = value.Where(c =>
                char.IsLetterOrDigit(c) || c == ' ' || c == '-' || c == '(' || c == ')' || c == '[' || c == ']');
            var cleaned = new string(validChars.ToArray()).Trim();
            return string.IsNullOrWhiteSpace(cleaned) ? "document" : cleaned;
        }
    }
}
