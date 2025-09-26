using Ophelia.AI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ophelia.AI.Interfaces
{
    public interface IChatHistoryStore
    {
        Task<List<ChatHistoryMessage>> GetHistoryAsync(string conversationId, int maxMessages);
        Task SaveMessageAsync(string conversationId, string role, string content);
        Task ClearHistoryAsync(string conversationId);
    }
}
