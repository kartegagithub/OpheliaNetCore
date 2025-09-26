using Ophelia.AI.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.AI.Interfaces
{
    public interface IEmbeddingService
    {
        Task<float[]> GenerateEmbeddingAsync(string text);
        Task<List<float[]>> GenerateEmbeddingsAsync(List<string> texts);
        Task<List<EmbeddingResult>> GenerateEmbeddingsWithMetadataAsync(List<string> texts);
        int GetEmbeddingDimension();
    }
}
