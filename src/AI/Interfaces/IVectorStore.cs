using Ophelia.AI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ophelia.AI.Interfaces
{
    public interface IVectorStore
    {
        Task<List<VectorSearchResult>> SearchAsync(float[] embedding, int topK);
        Task UpsertAsync(List<VectorDocument> documents);
    }
}
