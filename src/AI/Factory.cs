using Ophelia.AI.ChatServices;
using Ophelia.AI.Interfaces;
using Ophelia.AI.Models;
using System;
using System.Collections.Generic;
using System.Text;

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
                default:
                    break;
            }
            throw new NotImplementedException($"LLM Type {config.LLMConfig.Type} not implemented");
        }
    }
}
