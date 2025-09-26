using System;

namespace Ophelia.AI.Models
{
    public class ChatHistoryMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
