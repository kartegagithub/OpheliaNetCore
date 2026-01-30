using Ophelia.AI.Interfaces;
using Ophelia.AI.Models;

namespace Ophelia.AI.ChatServices
{
    public class GroqChatService : OpenAICompatibleChatService
    {
        public GroqChatService(AIConfig configuration, IChatHistoryStore chatHistoryStore) 
            : base(configuration, chatHistoryStore, "https://api.groq.com/openai/v1")
        {
        }
    }
}
