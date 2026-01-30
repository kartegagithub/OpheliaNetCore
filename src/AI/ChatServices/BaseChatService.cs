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
        public abstract Task CompleteChatStreamingAsync(string userMessage, Action<string, string> outputAction, string? userId = null);

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
        protected async Task<(List<VectorSearchResult> chunks, List<ChatHistoryMessage> history)> PrepareContextAsync(string userMessage, string conversationId)
        {
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(userMessage);
            var relevantChunks = await _vectorStore.SearchAsync(queryEmbedding, this._configuration.MaxRetrievedChunks);
            List<ChatHistoryMessage> chatHistory = new List<ChatHistoryMessage>();
            if (this.ChatHistoryStore != null)
                chatHistory = await this.ChatHistoryStore.GetHistoryAsync(conversationId, this._configuration.MaxChatHistoryMessages);

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
            await this.UploadFileAsync(System.IO.Path.GetFileName(filePath), File.ReadAllBytes(filePath));
        }

        public async Task UploadFileAsync(string fileName, byte[] fileData)
        {
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
                            Source = $"{fileName}"
                        }
                    }).Wait();
                    counter++;
                }
                
            }
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
            switch (config.LLMConfig.Type)
            {
                case LLMType.OpenAI:
                case LLMType.Groq:
                case LLMType.DeepSeek:
                case LLMType.Ollama:
                case LLMType.LMStudio:
                    return new OpenAIEmbeddingService(config);
                case LLMType.AzureOpenAI:
                    return new AzureOpenAIEmbeddingService(config);
                case LLMType.Claude:
                    return new ClaudeEmbeddingService(config);
                case LLMType.Gemini:
                case LLMType.Zai:
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
                    return new PineconeService(config);
                case VectorDbType.Redis:
                    return new RedisService(config);
                case VectorDbType.ElasticSearch:
                    return new ElasticSearchService(config);
            }
            throw new NotImplementedException($"LLM Type {config.LLMConfig.Type} not implemented");
        }
    }
}
