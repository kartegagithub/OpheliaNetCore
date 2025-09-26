using System;
using System.Collections.Generic;
using System.Text;

namespace Ophelia.AI.Models
{
    public class AIConfig
    {
        public LLMConfig? LLMConfig { get; set; }
        public int MaxChatHistoryMessages { get; set; } = 20;
        public int MaxRetrievedChunks { get; set; } = 5;
    }

    public class LLMConfig
    {
        public LLMType Type { get; set; } = LLMType.OpenAI;
        public string Endpoint { get; set; } = "";
        public string APIKey { get; set; } = string.Empty;
        public string Model { get; set; } = "gpt-4-turbo-preview";

        /// <summary>
        /// Use {context} placeholder for context definition
        /// </summary>
        public string SystemPrompt { get; set; } = "";
        public int MaxBatchSize { get; set; } = 100;
        public bool EnableCache { get; set; } = true;
        public int CacheExpirationHours { get;set; } = 24;
    }

    public class VectorConfig
    {
        public VectorDbType Type { get; set; } = VectorDbType.Pinecone;
        public string Endpoint { get; set; } = "";
        public string APIKey { get; set; } = string.Empty;
    }
}
