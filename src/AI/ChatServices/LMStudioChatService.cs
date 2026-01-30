using Ophelia.AI.Interfaces;
using Ophelia.AI.Models;

namespace Ophelia.AI.ChatServices
{
    public class LMStudioChatService : OpenAICompatibleChatService
    {
        public LMStudioChatService(AIConfig configuration, IChatHistoryStore chatHistoryStore) 
            : base(configuration, chatHistoryStore, "http://localhost:1234/v1")
        {
        }
    }
}
