using System;
using System.Collections.Generic;

namespace Ophelia.AI.Models
{
    public class VectorDocument
    {
        public string Id { get; set; } = string.Empty;
        public float[] Embedding { get; set; } = Array.Empty<float>();
        public string Content { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
}
