using Ophelia.AI.EmbeddingServices;
using Ophelia.AI.Interfaces;
using Ophelia.AI.Models;
using Ophelia.AI.VectorServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.AI.ChatServices
{
    public abstract class BaseChatService : IChatService, IDisposable
    {
        private IVectorStore? _vectorStore;
        private IEmbeddingService? _embeddingService;
        private AIConfig _configuration;
        private IChatHistoryStore? _chatHistoryStore;

        public AIConfig Config => _configuration;
        public IChatHistoryStore? ChatHistoryStore => _chatHistoryStore;

        protected BaseChatService(
            AIConfig configuration,
            IChatHistoryStore chatHistoryStore)
        {
            _configuration = configuration;
            _chatHistoryStore = chatHistoryStore;

            _vectorStore = this.CreateVectorStore(configuration);
            _embeddingService = this.CreateEmbedingService(configuration);
        }

        public abstract Task<ChatResponse> CompleteChatAsync(string userMessage, string? userId = null, Dictionary<string, string>? filter = null, List<ChatAttachment>? attachments = null);
        public abstract Task CompleteChatStreamingAsync(string userMessage, Action<string, string> outputAction, string? userId = null, Dictionary<string, string>? filter = null, List<ChatAttachment>? attachments = null);

        public async Task<IEnumerable<ChatHistoryMessage>> GetChatHistoryAsync(string userId)
        {
            if (this.ChatHistoryStore == null) return null;

            var history = await this.ChatHistoryStore.GetHistoryAsync(userId, 50);
            return history;
        }

        public async Task ClearChatHistoryAsync(string userId)
        {
            if (this.ChatHistoryStore == null) return;
            await this.ChatHistoryStore.ClearHistoryAsync(userId);
        }

        // Common helper methods
        protected async Task<(List<VectorSearchResult> chunks, List<ChatHistoryMessage> history)> PrepareContextAsync(string userMessage, string conversationId, Dictionary<string, string>? filter = null)
        {
            float[] queryEmbedding = null;
            if (_embeddingService != null)
            {
                try
                {
                    queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(userMessage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            List<VectorSearchResult> relevantChunks = new List<VectorSearchResult>();
            if (queryEmbedding != null && _vectorStore != null)
            {
                try
                {
                    relevantChunks = await _vectorStore.SearchAsync(queryEmbedding, this._configuration.MaxRetrievedChunks, filter);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            List<ChatHistoryMessage> chatHistory = new List<ChatHistoryMessage>();
            if (this.ChatHistoryStore != null)
                chatHistory = await this.ChatHistoryStore.GetHistoryAsync(conversationId, this._configuration.MaxChatHistoryMessages);

            return (relevantChunks, chatHistory);
        }

        protected string BuildContext(List<VectorSearchResult> chunks)
        {
            if (!chunks.Any())
                return "";

            var contextBuilder = new StringBuilder();
            contextBuilder.AppendLine();

            for (int i = 0; i < chunks.Count; i++)
            {
                contextBuilder.AppendLine($"[Kaynak {i + 1}: {chunks[i].Source}]");
                contextBuilder.AppendLine(chunks[i].Content);
                contextBuilder.AppendLine();
            }

            return contextBuilder.ToString();
        }

        protected string GetSystemPrompt(string context)
        {
            return _configuration.LLMConfig.SystemPrompt?.Replace("{context}", context);
        }

        public virtual void Dispose()
        {
            this._configuration = null;
            this._vectorStore = null;
            this._chatHistoryStore = null;
            this._embeddingService = null;
            GC.SuppressFinalize(this);
        }

        public async Task UploadFileAsync(string filePath, Dictionary<string, string>? metadata = null)
        {
            await this.UploadFileAsync(System.IO.Path.GetFileName(filePath), File.ReadAllBytes(filePath), metadata);
        }

        public async Task UploadFileAsync(string fileName, byte[] fileData, Dictionary<string, string>? metadata = null)
        {
            if (this.Config.VectorConfig == null || _embeddingService == null || _vectorStore == null)
                return;

            var fileContent = CleanText(Ophelia.Integration.Documents.DocumentParserService.ExtractText(fileName, fileData));
            if (!string.IsNullOrEmpty(fileContent))
            {
                var lines = fileContent.SplitToLines(this.Config.VectorConfig.Dimension);

                var data = await this._embeddingService.GenerateEmbeddingsAsync(lines);
                var counter = 0;
                foreach (var item in data)
                {
                    this._vectorStore.UpsertAsync(new List<VectorDocument>() {
                        new VectorDocument(){
                            Id = $"{fileName}_{counter}",
                            Content = lines[counter],
                            Embedding = item,
                            Source = $"{fileName}",
                            Metadata = metadata ?? new Dictionary<string, string>()
                        }
                    }).Wait();
                    counter++;
                }

            }
        }

        protected static string GetAttachmentFileName(ChatAttachment attachment)
        {
            if (!string.IsNullOrWhiteSpace(attachment.FileName))
                return attachment.FileName;

            if (!string.IsNullOrWhiteSpace(attachment.ProviderFileId))
                return attachment.ProviderFileId;

            if (!string.IsNullOrWhiteSpace(attachment.ProviderFileUri))
                return attachment.ProviderFileUri;

            if (!string.IsNullOrWhiteSpace(attachment.FileUrl))
                return attachment.FileUrl;

            return "attachment";
        }

        protected static string GetAttachmentMimeType(ChatAttachment attachment)
        {
            if (!string.IsNullOrWhiteSpace(attachment.MimeType))
                return attachment.MimeType;

            var extension = Path.GetExtension(attachment.FileName)?.TrimStart('.').ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(extension))
                return "application/octet-stream";

            return extension switch
            {
                "pdf" => "application/pdf",
                "txt" => "text/plain",
                "md" => "text/markdown",
                "json" => "application/json",
                "csv" => "text/csv",
                "tsv" => "text/tab-separated-values",
                "xml" => "application/xml",
                "html" => "text/html",
                "htm" => "text/html",
                "doc" => "application/msword",
                "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "xls" => "application/vnd.ms-excel",
                "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "png" => "image/png",
                "jpg" => "image/jpeg",
                "jpeg" => "image/jpeg",
                "webp" => "image/webp",
                "gif" => "image/gif",
                _ => "application/octet-stream"
            };
        }

        protected static byte[]? GetAttachmentBytes(ChatAttachment attachment)
        {
            if (attachment.FileData != null && attachment.FileData.Length > 0)
                return attachment.FileData;

            if (string.IsNullOrWhiteSpace(attachment.Base64Data))
                return null;

            try
            {
                return Convert.FromBase64String(attachment.Base64Data);
            }
            catch
            {
                return null;
            }
        }

        protected static string? GetAttachmentBase64(ChatAttachment attachment)
        {
            if (!string.IsNullOrWhiteSpace(attachment.Base64Data))
                return attachment.Base64Data;

            var bytes = GetAttachmentBytes(attachment);
            if (bytes == null || bytes.Length == 0)
                return null;

            return Convert.ToBase64String(bytes);
        }

        protected static bool IsImageMimeType(string mimeType)
        {
            return !string.IsNullOrWhiteSpace(mimeType) &&
                mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
        }

        protected static string BuildAttachmentSummary(List<ChatAttachment>? attachments)
        {
            if (attachments == null || attachments.Count == 0)
                return string.Empty;

            var lines = attachments.Select(item =>
            {
                var fileName = GetAttachmentFileName(item);
                var mimeType = GetAttachmentMimeType(item);
                var providerRef = item.ProviderFileId ?? item.ProviderFileUri ?? item.FileUrl;
                return string.IsNullOrWhiteSpace(providerRef)
                    ? $"- {fileName} ({mimeType})"
                    : $"- {fileName} ({mimeType}) [{providerRef}]";
            });

            return "\n\nAttached Files:\n" + string.Join("\n", lines);
        }

        protected static string AppendAttachmentSummary(string text, List<ChatAttachment>? attachments)
        {
            return text + BuildAttachmentSummary(attachments);
        }

        protected static string CleanText(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            while (text.IndexOf("\n\n") > -1 || text.IndexOf("\r\r") > -1 || text.IndexOf("  ") > -1)
            {
                text = text.Replace("\r ", "\r");
                text = text.Replace("\n\n", "\n");
                text = text.Replace("\r\r", "\r");
                text = text.Replace("  ", " ");
            }
            return text;
        }
        public virtual IEmbeddingService CreateEmbedingService(AIConfig config)
        {
            if(config.LLMConfig.UseLocalEmbeding)
                return new LocalOnnxEmbeddingService(config);

            switch (config.LLMConfig.Type)
            {
                case LLMType.OpenAI:
                case LLMType.Groq:
                case LLMType.DeepSeek:
                case LLMType.Ollama:
                case LLMType.LMStudio:
                case LLMType.Zai:
                    return new OpenAIEmbeddingService(config);
                case LLMType.AzureOpenAI:
                    return new AzureOpenAIEmbeddingService(config);
                case LLMType.Claude:
                    return new ClaudeEmbeddingService(config);
                case LLMType.Gemini:
                    return new GeminiEmbeddingService(config);
                case LLMType.HuggingFace:
                    return new HuggingFaceEmbeddingService(config);
                case LLMType.Custom:
                    break;
            }
            throw new NotImplementedException($"LLM Type {config.LLMConfig.Type} not implemented");
        }

        public virtual IVectorStore? CreateVectorStore(AIConfig config)
        {
            if (config.VectorConfig == null)
                return null;

            switch (config.VectorConfig.Type)
            {
                case VectorDbType.Pinecone:
                    return new PineconeService(config);
                case VectorDbType.Redis:
                    return new RedisService(config);
                case VectorDbType.ElasticSearch:
                    return new ElasticSearchService(config);
            }
            throw new NotImplementedException($"Vector DB Type {config.VectorConfig.Type} not implemented");
        }
    }
}
