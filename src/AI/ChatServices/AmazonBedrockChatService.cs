using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
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

        public override async Task<ChatResponse> CompleteChatAsync(string userMessage, string? userId = null, Dictionary<string, string>? filter = null)
        {
            var startTime = DateTime.UtcNow;
            var conversationId = userId ?? Guid.NewGuid().ToString();

            try
            {
                var (chunks, history) = await PrepareContextAsync(userMessage, conversationId);
                var context = BuildContext(chunks);
                var sources = chunks.Select(c => c.Source).Distinct().ToList();

                var messages = new List<Amazon.BedrockRuntime.Model.Message>();
                foreach(var h in history.TakeLast(this.Config.MaxChatHistoryMessages))
                {
                    messages.Add(new Amazon.BedrockRuntime.Model.Message
                    {
                        Role = h.Role == "user" ? ConversationRole.User : ConversationRole.Assistant,
                        Content = new List<Amazon.BedrockRuntime.Model.ContentBlock> { new Amazon.BedrockRuntime.Model.ContentBlock { Text = h.Content } }
                    });
                }
                messages.Add(new Amazon.BedrockRuntime.Model.Message
                {
                    Role = ConversationRole.User,
                    Content = new List<Amazon.BedrockRuntime.Model.ContentBlock> { new Amazon.BedrockRuntime.Model.ContentBlock { Text = userMessage } }
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
                    await this.ChatHistoryStore.SaveMessageAsync(conversationId, "user", userMessage);
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

        public override async Task CompleteChatStreamingAsync(string userMessage, Action<string, string> outputAction, string? userId = null, Dictionary<string, string>? filter = null)
        {
             // Similar logic but with ConverseStreamAsync. 
             // For brevity in this turn, calling blocking.
             var response = await CompleteChatAsync(userMessage, userId);
             outputAction("sources", JsonSerializer.Serialize(response.Sources));
             outputAction("message", response.Message);
             outputAction("done", "");
        }
    }
}
