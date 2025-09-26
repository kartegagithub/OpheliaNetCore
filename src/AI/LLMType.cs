namespace Ophelia.AI
{
    public enum LLMType: int
    {
        OpenAI = 1,
        AzureOpenAI = 2,
        Claude = 3,
        Gemini = 4,
        HuggingFace = 5,
        Custom = 999
    }
}
