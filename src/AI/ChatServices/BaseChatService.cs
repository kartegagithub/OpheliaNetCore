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

        public abstract Task<ChatResponse> CompleteChatAsync(string userMessage, string? userId = null);
        public abstract Task CompleteChatStreamingAsync(string userMessage, Stream outputStream, string? userId = null);

        public async Task<IEnumerable<ChatHistoryMessage>> GetChatHistoryAsync(string userId)
        {
            if (_chatHistoryStore == null) return null;

            var history = await _chatHistoryStore.GetHistoryAsync(userId, 50);
            return history;
        }

        public async Task ClearChatHistoryAsync(string userId)
        {
            if (_chatHistoryStore == null) return;
            await _chatHistoryStore.ClearHistoryAsync(userId);
        }

        // Common helper methods
        protected async Task<(List<VectorSearchResult> chunks, List<ChatHistoryMessage> history)> PrepareContextAsync(string userMessage, string conversationId)
        {
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(userMessage);
            var relevantChunks = await _vectorStore.SearchAsync(queryEmbedding, this._configuration.MaxRetrievedChunks);
            var chatHistory = await _chatHistoryStore.GetHistoryAsync(conversationId, this._configuration.MaxChatHistoryMessages);
            return (relevantChunks, chatHistory);
        }

        protected string BuildContext(List<VectorSearchResult> chunks)
        {
            if (!chunks.Any())
                return "Şu anda bu konuyla ilgili doküman bilgisi bulunamadı.";

            var contextBuilder = new StringBuilder();
            contextBuilder.AppendLine("İlgili Doküman Bilgileri:");
            contextBuilder.AppendLine();

            for (int i = 0; i < chunks.Count; i++)
            {
                contextBuilder.AppendLine($"[Kaynak {i + 1}: {chunks[i].Source}]");
                contextBuilder.AppendLine(chunks[i].Content);
                contextBuilder.AppendLine();
            }

            return contextBuilder.ToString();
        }

        protected async Task SendSseEventAsync(StreamWriter writer, string eventType, string data)
        {
            await writer.WriteLineAsync($"event: {eventType}");
            await writer.WriteLineAsync($"data: {data}");
            await writer.WriteLineAsync();
            await writer.FlushAsync();
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

        public async Task UploadFileAsync(string filePath)
        {
            var fileContent = Ophelia.Integration.Documents.DocumentParserService.ExtractText(System.IO.Path.GetFileName(filePath), File.ReadAllBytes(filePath));
            var data = await this._embeddingService.GenerateEmbeddingAsync(fileContent);
            this._vectorStore.UpsertAsync(new List<VectorDocument>() {
                new VectorDocument(){
                   Id = System.IO.Path.GetFileName(filePath),
                   Content = fileContent,
                   Embedding = data,
                   Source = filePath
                }
            }).Wait();
        }

        public async Task UploadFileAsync(string fileName, byte[] fileData)
        {
            var fileContent = Ophelia.Integration.Documents.DocumentParserService.ExtractText(fileName, fileData);
            var data = await this._embeddingService.GenerateEmbeddingAsync(fileContent);
            this._vectorStore.UpsertAsync(new List<VectorDocument>() {
                new VectorDocument(){
                   Id = fileName,
                   Content = fileContent,
                   Embedding = data,
                   Source = fileName
                }
            }).Wait();
        }

        public virtual IEmbeddingService CreateEmbedingService(AIConfig config)
        {
            switch (config.LLMConfig.Type)
            {
                case LLMType.OpenAI:
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

        public virtual IVectorStore CreateVectorStore(AIConfig config)
        {
            switch (config.VectorConfig.Type)
            {
                case VectorDbType.Pinecone:
                    return new PineconeService(config.VectorConfig.APIKey, config.VectorConfig.IndexName, config.VectorConfig.Endpoint, config.VectorConfig.UseSSL);
                case VectorDbType.Redis:
                    return new RedisService(config.VectorConfig.Endpoint.Split(","), config.VectorConfig.IndexName, config.VectorConfig.UserName, config.VectorConfig.Password);
                case VectorDbType.ElasticSearch:
                    return new ElasticSearchService(config.VectorConfig.Endpoint, config.VectorConfig.IndexName, config.VectorConfig.UserName, config.VectorConfig.Password);
            }
            throw new NotImplementedException($"LLM Type {config.LLMConfig.Type} not implemented");
        }
    }
}
