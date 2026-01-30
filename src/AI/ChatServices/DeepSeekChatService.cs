using Ophelia.AI.Interfaces;
using Ophelia.AI.Models;

namespace Ophelia.AI.ChatServices
{
    public class DeepSeekChatService : OpenAICompatibleChatService
    {
        public DeepSeekChatService(AIConfig configuration, IChatHistoryStore chatHistoryStore) 
            : base(configuration, chatHistoryStore, "https://api.deepseek.com")
        {
        }
    }
}
