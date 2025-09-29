using NRedisStack.RedisStackCommands;
using NRedisStack.Search;
using NRedisStack.Search.Literals.Enums;
using Ophelia.AI.Interfaces;
using Ophelia.AI.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NRedisStack.Search.Schema;

namespace Ophelia.AI.VectorServices
{
    public class RedisService : IDisposable, IVectorStore
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _db;
        private readonly string _indexName;
        private readonly string _keyPrefix;

        public RedisService(string[] endpoints, string indexName, string? username = null, string? password = null, int database = 0)
        {
            var config = new ConfigurationOptions();
            foreach (var item in endpoints)
            {
                if (!string.IsNullOrEmpty(item))
                    config.EndPoints.Add(item);
            }
            config.User = username;
            config.Password = password;
            _redis = ConnectionMultiplexer.Connect(config);
            _db = _redis.GetDatabase(database);
            _indexName = indexName;
            _keyPrefix = $"{indexName}:";
        }

        public void Dispose()
        {
            _redis?.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task<List<VectorSearchResult>> SearchAsync(float[] embedding, int topK)
        {
            try
            {
                var ft = _db.FT();

                var query = new Query($"*=>[KNN {topK} @embedding $vector AS score]")
                    .AddParam("vector", SerializeVector(embedding))
                    .SetSortBy("score")
                    .Limit(0, topK)
                    .ReturnFields("content", "score")
                    .Dialect(2);

                var searchResult = await ft.SearchAsync(_indexName, query);
                var results = new List<VectorSearchResult>();

                if (searchResult?.Documents != null)
                {
                    foreach (var doc in searchResult.Documents)
                    {
                        var content = doc["content"].ToString() ?? string.Empty;
                        var scoreStr = doc["score"].ToString();
                        float.TryParse(scoreStr, out var score);
                        results.Add(new VectorSearchResult
                        {
                            Content = content,
                            Score = score
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
                const int batchSize = 100;
                var batches = documents
                    .Select((doc, i) => new { doc, i })
                    .GroupBy(x => x.i / batchSize)
                    .Select(g => g.Select(x => x.doc).ToList())
                    .ToList();

                foreach (var batch in batches)
                {
                    var tasks = new List<Task>();

                    foreach (var doc in batch)
                    {
                        var key = $"{_keyPrefix}{doc.Id ?? Guid.NewGuid().ToString()}";

                        var hashEntries = new HashEntry[]
                        {
                            new HashEntry("content", doc.Content ?? string.Empty),
                            new HashEntry("embedding", SerializeVector(doc.Embedding)),
                            new HashEntry("timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                        };

                        tasks.Add(_db.HashSetAsync(key, hashEntries));
                    }

                    await Task.WhenAll(tasks);

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
        /// Index oluşturur (ilk kurulum için)
        /// </summary>
        public async Task CreateIndexAsync(int dimension)
        {
            try
            {
                var ft = _db.FT();

                var schema = new Schema()
                    .AddTextField("content")
                    .AddNumericField("timestamp")
                    .AddVectorField(
                        "embedding",
                        VectorField.VectorAlgo.HNSW,
                        new Dictionary<string, object>
                        {
                            { "TYPE", "FLOAT32" },
                            { "DIM", dimension },
                            { "DISTANCE_METRIC", "COSINE" }
                        }
                    );

                await ft.CreateAsync(
                    _indexName,
                    new FTCreateParams()
                        .On(IndexDataType.HASH)
                        .Prefix($"{_keyPrefix}"),
                    schema
                );
            }
            catch (Exception ex)
            {
                // Index zaten varsa hata vermemesi için
                if (!ex.Message.Contains("Index already exists"))
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Float array'i byte array'e dönüştürür
        /// </summary>
        private static byte[] SerializeVector(float[] vector)
        {
            var bytes = new byte[vector.Length * sizeof(float)];
            Buffer.BlockCopy(vector, 0, bytes, 0, bytes.Length);
            return bytes;
        }

        /// <summary>
        /// Index'teki toplam döküman sayısını döndürür
        /// </summary>
        public async Task<long> GetDocumentCountAsync()
        {
            var ft = _db.FT();
            var info = await ft.InfoAsync(_indexName);

            // Info sonucundan döküman sayısını parse et
            return info?.NumDocs ?? 0;
        }

        /// <summary>
        /// Belirli bir ID'ye sahip dökümanı siler
        /// </summary>
        public async Task<bool> DeleteAsync(string id)
        {
            var key = $"{_keyPrefix}{id}";
            return await _db.KeyDeleteAsync(key);
        }
    }
}
