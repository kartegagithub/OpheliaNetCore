using System.Collections.Generic;

namespace Ophelia.AI.Models
{
    /// <summary>
    /// Represents a file attachment that can be sent to providers and/or persisted in chat history.
    /// </summary>
    public class ChatAttachment
    {
        public string FileName { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public byte[]? FileData { get; set; }
        public string? Base64Data { get; set; }
        public string? FileUrl { get; set; }
        public string? ProviderFileId { get; set; }
        public string? ProviderFileUri { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
}
