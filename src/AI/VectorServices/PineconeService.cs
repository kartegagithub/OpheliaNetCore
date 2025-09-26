using Ophelia.AI.Interfaces;
using Ophelia.AI.Models;
using Pinecone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ophelia.AI.VectorServices
{
    public class PineconeService : IDisposable, IVectorStore
    {
        private PineconeClient _pineconeClient;
        private readonly string _indexName;

        // Cloud Pinecone constructor
        public PineconeService(string apiKey, string indexName)
        {
            _pineconeClient = new PineconeClient(apiKey, new ClientOptions());
            _indexName = indexName;
        }

        // Local Pinecone constructor
        public PineconeService(string apiKey, string indexName, string hostUrl, bool useHttps = false)
        {
            var clientOptions = new ClientOptions();

            if (!string.IsNullOrEmpty(hostUrl))
            {
                var baseUri = new Uri(hostUrl);
                if (!useHttps && baseUri.Scheme == "https")
                {
                    hostUrl = hostUrl.Replace("https://", "http://");
                }
                else if (useHttps && baseUri.Scheme == "http")
                {
                    hostUrl = hostUrl.Replace("http://", "https://");
                }
                clientOptions.BaseUrl = hostUrl;
            }
            _pineconeClient = new PineconeClient(apiKey, clientOptions);
            _indexName = indexName;
        }

        public void Dispose()
        {
            _pineconeClient = null;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Pinecone index'inde verilen embedding'e en benzer top K vektörü arar.
        /// Local ve cloud deployment'lar için uyumlu.
        /// </summary>
        /// <param name="embedding">Arama yapılacak embedding</param>
        /// <param name="topK">Döndürülecek sonuç sayısı</param>
        /// <returns>VectorSearchResult listesi</returns>
        public async Task<List<VectorSearchResult>> SearchAsync(float[] embedding, int topK)
        {
            try
            {
                var index = _pineconeClient.Index(_indexName);

                var queryRequest = new QueryRequest
                {
                    Vector = embedding,
                    TopK = (uint)topK,
                    IncludeMetadata = true,
                    IncludeValues = false // Performans için values'ları dahil etme
                };

                var queryResponse = await index.QueryAsync(queryRequest);
                var results = new List<VectorSearchResult>();

                if (queryResponse?.Matches != null)
                {
                    foreach (var match in queryResponse.Matches)
                    {
                        if (match.Metadata != null &&
                            match.Metadata.TryGetValue("content", out var contentValue))
                        {
                            results.Add(new VectorSearchResult
                            {
                                Content = contentValue?.ToString() ?? string.Empty,
                                Score = match.Score ?? 0f
                            });
                        }
                    }
                }

                return results;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Vektör dökümanlarını Pinecone index'ine ekler veya günceller.
        /// Local ve cloud deployment'lar için uyumlu.
        /// </summary>
        /// <param name="documents">Eklenmek istenen VectorDocument listesi</param>
        public async Task UpsertAsync(List<VectorDocument> documents)
        {
            if (documents == null || !documents.Any())
            {
                return;
            }

            try
            {
                var index = _pineconeClient.Index(_indexName);

                // Batch boyutunu kontrol et (Pinecone limitleri için)
                const int batchSize = 100;
                var batches = documents
                    .Select((doc, i) => new { doc, i })
                    .GroupBy(x => x.i / batchSize)
                    .Select(g => g.Select(x => x.doc).ToList())
                    .ToList();

                foreach (var batch in batches)
                {
                    var vectors = batch.Select(doc => new Vector
                    {
                        Id = doc.Id ?? Guid.NewGuid().ToString(),
                        Values = doc.Embedding,
                        Metadata = new Metadata
                        {
                            ["content"] = doc.Content ?? string.Empty,
                            ["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                        }
                    }).ToList();

                    await index.UpsertAsync(new UpsertRequest
                    {
                        Vectors = vectors
                    });

                    // Rate limiting için kısa bekleme (özellikle local deployment'da yararlı)
                    if (batches.Count > 1)
                    {
                        await Task.Delay(100);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}