namespace Ophelia.AI.Models
{
    public class AIConfig
    {
        public LLMConfig? LLMConfig { get; set; }
        public VectorConfig? VectorConfig { get; set; }
        public int MaxChatHistoryMessages { get; set; } = 20;
        public int MaxRetrievedChunks { get; set; } = 5;
    }

    public class LLMConfig
    {
        public LLMType Type { get; set; } = LLMType.OpenAI;
        public string Endpoint { get; set; } = "";
        public string APIKey { get; set; } = string.Empty;

        /// <summary>
        /// Claude: claude-3-sonnet-20240229, claude-3-haiku-20240307
        /// Gemini: text-bison-001, gemini-1.5-pro, gemini-1.5-flash, gemini-1.5-turbo, gemini-1.0-pro, gemini-1.0-turbo
        /// OpenAI: gpt-4o, gpt-4o-mini, gpt-4-turbo, gpt-4, gpt-3.5-turbo
        /// </summary>
        public string Model { get; set; } = "";

        /// <summary>
        /// Gemini: gemini-embedding-001
        /// Claude: claude-3-sonnet-20240229, claude-3-haiku-20240307
        /// OpenAI: text-embedding-3-small, text-embedding-3-large, text-embedding-ada-002
        /// </summary>
        public string EmbedingModel { get; set; } = "";

        /// <summary>
        /// Use LocalOnnx Embeding Model
        /// </summary>
        public bool UseLocalEmbeding { get; set; }

        /// <summary>
        /// LocalOnnx Embeding Model Path
        /// </summary>
        public string LocalModelPath { get; set; } = "";

        /// <summary>
        /// LocalOnnx Tokenizer Model Path
        /// </summary>
        public string TokenizerPath { get; set; } = "";

        /// <summary>
        /// Use {context} placeholder for context definition
        /// </summary>
        public string SystemPrompt { get; set; } = "";
        
        public int MaxBatchSize { get; set; } = 100;
        public bool EnableCache { get; set; } = true;
        public int CacheExpirationHours { get; set; } = 24;
        public int MaxTokens { get; set; } = 512;
        public double Temperature { get; internal set; } = 0.7;
        public double TopP { get; internal set; } = 0.9;
        //public int MaxDocumentByteCount { get; set; } = 36000;
    }

    public class VectorConfig
    {
        public VectorDbType Type { get; set; } = VectorDbType.Pinecone;
        public string Endpoint { get; set; } = "";
        public string APIKey { get; set; } = string.Empty;

        /// <summary>
        /// If you use Elastic Search, please see <see cref="https://discuss.elastic.co/t/index-name-must-be-lowercase-after-upgrade-to-7-9-2/251433" />
        /// </summary>
        public string IndexName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool UseSSL { get; set; } = true;

        public int Dimension { get; set; } = 1536;
    }
}
