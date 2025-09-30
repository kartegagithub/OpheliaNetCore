using Nest;
using Ophelia.AI.Interfaces;
using Ophelia.AI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.AI.VectorServices
{
    public class ElasticSearchService : IDisposable, IVectorStore
    {
        private readonly ElasticClient _client;
        private readonly string _indexName;
        private bool _indexChecked;
        private readonly int _embeddingDimension;

        public ElasticSearchService(AIConfig config)
        {
            var settings = new ConnectionSettings(new Uri(config.VectorConfig.Endpoint))
                .DefaultIndex(_indexName = config.VectorConfig.IndexName);

            if (!string.IsNullOrEmpty(config.VectorConfig.UserName))
            {
                settings.BasicAuthentication(config.VectorConfig.UserName, config.VectorConfig.Password);
            }

            _client = new ElasticClient(settings);
            this._embeddingDimension = config.VectorConfig.Dimension;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public async Task<List<VectorSearchResult>> SearchAsync(float[] embedding, int topK)
        {
            try
            {
                await EnsureIndexExistsAsync();

                var searchResponse = await _client.SearchAsync<VectorDoc>(s => s
                    .Index(_indexName)
                    .Size(topK)
                    .Query(q => q
                        .ScriptScore(ss => ss
                            .Query(qq => qq.MatchAll())
                            .Script(script => script
                                .Source("cosineSimilarity(params.query_vector, 'embedding') + 1.0")
                                .Params(p => p.Add("query_vector", embedding))
                            )
                        )
                    )
                );

                var results = new List<VectorSearchResult>();

                if (searchResponse.Documents != null)
                {
                    foreach (var doc in searchResponse.Documents)
                    {
                        results.Add(new VectorSearchResult
                        {
                            Content = doc.Content ?? string.Empty,
                            Score = doc.Score
                        });
                    }
                }

                return results;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UpsertAsync(List<VectorDocument> documents)
        {
            if (documents == null || !documents.Any())
            {
                return;
            }

            try
            {
                await EnsureIndexExistsAsync();

                const int batchSize = 100;
                var batches = documents
                    .Select((doc, i) => new { doc, i })
                    .GroupBy(x => x.i / batchSize)
                    .Select(g => g.Select(x => x.doc).ToList())
                    .ToList();

                foreach (var batch in batches)
                {
                    var bulkDescriptor = new BulkDescriptor();

                    foreach (var doc in batch)
                    {
                        var esDoc = new VectorDoc
                        {
                            Id = doc.Id ?? Guid.NewGuid().ToString(),
                            Embedding = doc.Embedding,
                            Content = doc.Content ?? string.Empty,
                            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                        };

                        bulkDescriptor.Index<VectorDoc>(i => i
                            .Document(esDoc)
                            .Id(esDoc.Id)
                            .Index(_indexName)
                        );
                    }

                    var bulkResponse = await _client.BulkAsync(bulkDescriptor);

                    if (!bulkResponse.IsValid)
                    {
                        Console.WriteLine($"Bulk upsert is not valid: {bulkResponse.DebugInformation}");
                    }

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

        /// <summary>
        /// Index yoksa otomatik oluşturur (thread-safe)
        /// </summary>
        private async Task EnsureIndexExistsAsync()
        {
            if (_indexChecked)
                return;

            var existsResponse = await _client.Indices.ExistsAsync(_indexName);

            if (!existsResponse.Exists)
            {
                await CreateIndexAsync(_embeddingDimension);
            }

            _indexChecked = true;
        }

        /// <summary>
        /// Index oluşturur (ilk kurulum için)
        /// </summary>
        public async Task CreateIndexAsync(int dimension)
        {
            var createIndexResponse = await _client.Indices.CreateAsync(_indexName, c => c
                .Map<VectorDoc>(m => m
                    .Properties(ps => ps
                        .Keyword(k => k.Name(n => n.Id))
                        .DenseVector(dv => dv
                            .Name(n => n.Embedding)
                            .Dimensions(dimension)
                        )
                        .Text(t => t.Name(n => n.Content))
                        .Number(n => n
                            .Name(nn => nn.Timestamp)
                            .Type(NumberType.Long)
                        )
                    )
                )
            );

            if (!createIndexResponse.IsValid)
            {
                throw new Exception($"Index creation failed: {createIndexResponse.DebugInformation}");
            }
        }

        // Internal document class
        private class VectorDoc
        {
            public string Id { get; set; }
            public float[] Embedding { get; set; }
            public string Content { get; set; }
            public long Timestamp { get; set; }
            public float Score { get; set; }
        }
    }
}
