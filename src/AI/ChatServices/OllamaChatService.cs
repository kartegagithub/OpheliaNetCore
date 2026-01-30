using Ophelia.AI.Interfaces;
using Ophelia.AI.Models;

namespace Ophelia.AI.ChatServices
{
    public class OllamaChatService : OpenAICompatibleChatService
    {
        public OllamaChatService(AIConfig configuration, IChatHistoryStore chatHistoryStore) 
            : base(configuration, chatHistoryStore, "http://localhost:11434/v1")
        {
        }
    }
}
