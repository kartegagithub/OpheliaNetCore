using Ophelia.AI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Ophelia.AI.Interfaces
{
    public interface IChatService
    {
        AIConfig Config { get; }
        Task<ChatResponse> CompleteChatAsync(string userMessage, string? userId = null);
        Task CompleteChatStreamingAsync(string userMessage, Action<string, string> outputAction, string? userId = null);
        Task<IEnumerable<ChatHistoryMessage>> GetChatHistoryAsync(string userId);
        Task ClearChatHistoryAsync(string userId);
        Task UploadFileAsync(string filePath);
        Task UploadFileAsync(string fileName, byte[] fileData);
        IVectorStore CreateVectorStore(AIConfig config);
        IEmbeddingService CreateEmbedingService(AIConfig config);
    }
}
