using System;
using System.Collections.Generic;

namespace Ophelia.AI.Models
{
    public class ChatHistoryMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public List<ChatAttachment> Attachments { get; set; } = new List<ChatAttachment>();
    }
}
