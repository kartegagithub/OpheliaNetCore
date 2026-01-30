namespace Ophelia.AI
{
    public enum LLMType : int
    {
        OpenAI = 1,
        AzureOpenAI = 2,
        Claude = 3,
        Gemini = 4,
        HuggingFace = 5,
        Zai = 6,
        Groq = 8,
        Cohere = 9,
        DeepSeek = 10,
        AmazonBedrock = 11,
        Ollama = 12,
        LMStudio = 13,
        Custom = 999
    }
}
