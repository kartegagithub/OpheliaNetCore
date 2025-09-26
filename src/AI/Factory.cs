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
        public static IChatService CreateChatService(AIConfig config, IChatHistoryStore historyStore)
        {
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
                    return new ClaudeEmbeddingService(config.LLMConfig.APIKey);
                case LLMType.Gemini:
                    return new GeminiEmbeddingService(config.LLMConfig.APIKey);
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
            }
            throw new NotImplementedException($"LLM Type {config.LLMConfig.Type} not implemented");
        }
    }
}
