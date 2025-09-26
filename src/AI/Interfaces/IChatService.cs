using Ophelia.AI.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Ophelia.AI.Interfaces
{
    public interface IChatService
    {
        Task<ChatResponse> CompleteChatAsync(string userMessage, string? userId = null);
        Task CompleteChatStreamingAsync(string userMessage, Stream outputStream, string? userId = null);
        Task<IEnumerable<ChatHistoryMessage>> GetChatHistoryAsync(string userId);
        Task ClearChatHistoryAsync(string userId);
        Task UploadFileAsync(string filePath);
        Task UploadFileAsync(string fileName, byte[] fileData);
    }
}
