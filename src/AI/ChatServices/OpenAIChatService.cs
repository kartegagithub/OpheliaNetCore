using OpenAI.Chat;
using Ophelia;
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
    public class OpenAIChatService : BaseChatService
    {
        private readonly ChatClient _chatClient;

        public OpenAIChatService(AIConfig configuration, IChatHistoryStore chatHistoryStore) : base(configuration, chatHistoryStore)
        {
            _chatClient = new ChatClient(configuration.LLMConfig.Model, configuration.LLMConfig.APIKey);
        }

        public override async Task<ChatResponse> CompleteChatAsync(string userMessage, string? userId = null, Dictionary<string, string>? filter = null, List<ChatAttachment>? attachments = null)
        {
            var startTime = DateTime.UtcNow;
            var conversationId = userId ?? Guid.NewGuid().ToString();

            try
            {
                var contextData = await this.PrepareContextAsync(userMessage, userId, filter);

                // 4. Context oluştur
                var context = BuildContext(contextData.chunks);
                var sources = contextData.chunks.Select(c => c.Source).Distinct().ToList();

                // 5. LLM'e gönder
                var messages = BuildChatMessages(context, userMessage, contextData.history, attachments);

                var chatCompletion = await _chatClient.CompleteChatAsync(messages);
                var responseMessage = chatCompletion.Value.Content[0].Text;

                if(this.ChatHistoryStore != null)
                {
                    // 6. Chat geçmişini kaydet
                    await this.ChatHistoryStore.SaveMessageAsync(conversationId, "user", userMessage, attachments);
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
                // 1-3. Embedding ve context oluştur (aynı)
                var contextData = await this.PrepareContextAsync(userMessage, userId, filter);
                var context = BuildContext(contextData.chunks);

                // 4. Kaynakları gönder
                var sources = contextData.chunks.Select(c => c.Source).Distinct().ToList();
                outputAction("sources", JsonSerializer.Serialize(sources));

                // 5. Messages oluştur
                var messages = BuildChatMessages(context, userMessage, contextData.history, attachments);

                // 6. Streaming response
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

                if(this.ChatHistoryStore != null)
                {
                    // 7. Chat geçmişini kaydet
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


        private List<ChatMessage> BuildChatMessages(string context, string userMessage, List<ChatHistoryMessage> history, List<ChatAttachment>? attachments)
        {
            var messages = new List<ChatMessage>();

            // System prompt
            //["ChatBot:SystemPrompt"] ?? @"
            //Sen bir firma asistanısın ve sadece firma dokümanlarına dayalı bilgi veriyorsun.

            //KURALLAR:
            //1. Sadece verilen doküman bilgilerini kullan
            //2. Dokümanlarla ilgili olmayan sorulara 'Bu konuda elimde bilgi yok' yanıtını ver
            //3. Türkçe ve profesyonel bir dil kullan
            //4. Kesin bilmediğin şeyleri uydurma
            //5. Yanıtlarını kaynaklara dayandır

            //Firma dokümanları:
            //{context}";

            var systemPrompt = this.Config.LLMConfig.SystemPrompt.Replace("{context}", context);
            messages.Add(ChatMessage.CreateSystemMessage(systemPrompt));

            // Chat geçmişi
            foreach (var historyMsg in history.TakeLast(this.Config.MaxChatHistoryMessages))
            {
                if (historyMsg.Role == "user")
                    messages.Add(BuildUserMessage(historyMsg.Content, historyMsg.Attachments));
                else
                    messages.Add(ChatMessage.CreateAssistantMessage(historyMsg.Content));
            }

            // Mevcut kullanıcı mesajı
            messages.Add(BuildUserMessage(userMessage, attachments));

            return messages;
        }

        private ChatMessage BuildUserMessage(string userMessage, List<ChatAttachment>? attachments)
        {
            var contentParts = new List<ChatMessageContentPart>();
            var unsupported = new List<ChatAttachment>();

            if (attachments != null)
            {
                foreach (var attachment in attachments)
                {
                    if (!string.IsNullOrWhiteSpace(attachment.ProviderFileId))
                    {
                        contentParts.Add(CreateFilePartFromProviderFileId(attachment.ProviderFileId));
                        continue;
                    }

                    var bytes = GetAttachmentBytes(attachment);
                    if (bytes != null && bytes.Length > 0)
                    {
                        contentParts.Add(CreateFilePartFromBytes(
                            bytes,
                            GetAttachmentMimeType(attachment),
                            GetAttachmentFileName(attachment)));
                    }
                    else
                    {
                        unsupported.Add(attachment);
                    }
                }
            }

            contentParts.Insert(0, ChatMessageContentPart.CreateTextPart(AppendAttachmentSummary(userMessage, unsupported)));
            return ChatMessage.CreateUserMessage(contentParts);
        }

        private static ChatMessageContentPart CreateFilePartFromProviderFileId(string providerFileId)
        {
#pragma warning disable OPENAI001
            return ChatMessageContentPart.CreateFilePart(providerFileId);
#pragma warning restore OPENAI001
        }

        private static ChatMessageContentPart CreateFilePartFromBytes(byte[] bytes, string mimeType, string fileName)
        {
#pragma warning disable OPENAI001
            return ChatMessageContentPart.CreateFilePart(BinaryData.FromBytes(bytes), mimeType, fileName);
#pragma warning restore OPENAI001
        }
    }
}
