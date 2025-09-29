using Ophelia.AI.ChatServices;
using Ophelia.AI.EmbeddingServices;
using Ophelia.AI.Interfaces;
using Ophelia.AI.Models;
using Ophelia.AI.VectorServices;
using System;

namespace Ophelia.AI
{
    public static class Factory
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2208:Instantiate argument exceptions correctly", Justification = "<Pending>")]
        public static IChatService CreateChatService(AIConfig config, IChatHistoryStore? historyStore = null)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (config.LLMConfig == null)
                throw new ArgumentNullException(nameof(config.LLMConfig));
            if (string.IsNullOrEmpty(config.LLMConfig.APIKey))
                throw new ArgumentNullException(nameof(config.LLMConfig.APIKey), "API Key is required in LLMConfig");
            if (string.IsNullOrEmpty(config.LLMConfig.Model))
                throw new ArgumentNullException(nameof(config.LLMConfig.Model), "Model is required in LLMConfig");
            if (string.IsNullOrEmpty(config.LLMConfig.SystemPrompt))
                throw new ArgumentNullException(nameof(config.LLMConfig.SystemPrompt), "System prompt is required in LLMConfig");

            switch (config.LLMConfig.Type)
            {
                case LLMType.OpenAI:
                    return new OpenAIChatService(config, historyStore);
                case LLMType.AzureOpenAI:
                    return new AzureOpenAIChatService(config, historyStore);
                case LLMType.Claude:
                    return new ClaudeChatService(config, historyStore);
                case LLMType.Gemini:
                    return new GeminiChatService(config, historyStore);
                case LLMType.HuggingFace:
                    break;
                case LLMType.Custom:
                    break;
            }
            throw new NotImplementedException($"LLM Type {config.LLMConfig.Type} not implemented");
        }

        internal static IEmbeddingService CreateEmbedingService(AIConfig config)
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
                    break;
                case LLMType.Custom:
                    break;
            }
            throw new NotImplementedException($"LLM Type {config.LLMConfig.Type} not implemented");
        }

        internal static IVectorStore CreateVectorStore(AIConfig config)
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
