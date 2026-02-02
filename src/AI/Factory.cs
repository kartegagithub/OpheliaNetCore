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
                    return new HuggingFaceChatService(config, historyStore);
                case LLMType.Zai:
                    return new ZaiChatService(config, historyStore);
                case LLMType.Groq:
                    return new GroqChatService(config, historyStore);
                case LLMType.Cohere:
                    return new CohereChatService(config, historyStore);
                case LLMType.DeepSeek:
                    return new DeepSeekChatService(config, historyStore);
                case LLMType.AmazonBedrock:
                    return new AmazonBedrockChatService(config, historyStore);
                case LLMType.Ollama:
                    return new OllamaChatService(config, historyStore);
                case LLMType.LMStudio:
                    return new LMStudioChatService(config, historyStore);
                case LLMType.Custom:
                    break;
            }
            throw new NotImplementedException($"LLM Type {config.LLMConfig.Type} not implemented");
        }
    }
}
