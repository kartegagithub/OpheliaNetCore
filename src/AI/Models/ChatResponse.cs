using System;
using System.Collections.Generic;
using System.Text;

namespace Ophelia.AI.Models
{
    public class ChatResponse
    {
        public string Message { get; set; } = string.Empty;
        public List<string> Sources { get; set; } = new List<string>();
        public int TokensUsed { get; set; }
        public double ProcessingTimeMs { get; set; }
        public string ConversationId { get; set; } = string.Empty;
    }
}
