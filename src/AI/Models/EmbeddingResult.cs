using System;

namespace Ophelia.AI.Models
{
    public class EmbeddingResult
    {
        public float[] Embedding { get; set; } = Array.Empty<float>();
        public string Text { get; set; } = string.Empty;
        public int TokenCount { get; set; }
        public int Index { get; set; }
    }
}
