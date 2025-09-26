using Ophelia.AI.Models;
using Org.BouncyCastle.Asn1.Cmp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.AI.Interfaces
{
    public interface IChatService
    {
        Task<ChatResponse> ProcessQueryAsync(string userMessage, string? userId = null);
        Task ProcessQueryStreamAsync(string userMessage, Stream outputStream, string? userId = null);
        Task<IEnumerable<ChatHistoryMessage>> GetChatHistoryAsync(string userId);
        Task ClearChatHistoryAsync(string userId);
    }
}
