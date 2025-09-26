using OpenAI.Embeddings;
using Ophelia.AI.Interfaces;
using Ophelia.AI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ophelia.AI.EmbeddingServices
{
    public class AzureOpenAIEmbeddingService : IEmbeddingService
    {
        private readonly EmbeddingClient _embeddingClient;
        private readonly int _embeddingDimension;
        private readonly bool _enableCache;

        public AzureOpenAIEmbeddingService(AIConfig config)
        {
            var endpoint = config.LLMConfig.Endpoint ?? throw new InvalidOperationException("Azure OpenAI endpoint not configured");
            var apiKey = config.LLMConfig.APIKey ?? throw new InvalidOperationException("Azure OpenAI API key not configured");
            
            _embeddingClient = new EmbeddingClient(endpoint, new System.ClientModel.ApiKeyCredential(apiKey));
            _embeddingDimension = 1536; // Azure OpenAI text-embedding-ada-002
            _enableCache = config.LLMConfig.EnableCache;
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            // OpenAI implementasyonu ile aynı mantık
            var embedding = await _embeddingClient.GenerateEmbeddingAsync(text);
            return embedding.Value.ToFloats().ToArray();
        }

        public async Task<List<float[]>> GenerateEmbeddingsAsync(List<string> texts)
        {
            var embeddings = await _embeddingClient.GenerateEmbeddingsAsync(texts);
            return embeddings.Value.Select(e => e.ToFloats().ToArray()).ToList();
        }

        public async Task<List<EmbeddingResult>> GenerateEmbeddingsWithMetadataAsync(List<string> texts)
        {
            var embeddings = await _embeddingClient.GenerateEmbeddingsAsync(texts);
            return embeddings.Value.Select((e, index) => new EmbeddingResult
            {
                Embedding = e.ToFloats().ToArray(),
                Text = texts[index],
                TokenCount = e.Index,
                Index = index
            }).ToList();
        }

        public int GetEmbeddingDimension() => _embeddingDimension;
    }
}
