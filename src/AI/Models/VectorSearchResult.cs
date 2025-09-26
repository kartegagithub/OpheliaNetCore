using System.Collections.Generic;

namespace Ophelia.AI.Models
{
    public class VectorSearchResult
    {
        public string Content { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public float Score { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
}
