using OpenAI.Embeddings;
using Ophelia.AI.Interfaces;
using Ophelia.AI.Models;
using Ophelia.Caching;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.AI.EmbeddingServices
{
    public class OpenAIEmbeddingService : IEmbeddingService
    {
        private readonly EmbeddingClient _embeddingClient;

        private readonly int _embeddingDimension;
        private readonly int _maxBatchSize;
        private readonly bool _enableCache;
        private readonly int _cacheExpiration;

        public OpenAIEmbeddingService(AIConfig config)
        {
            var apiKey = config.LLMConfig.APIKey ?? throw new InvalidOperationException("OpenAI API key not configured");

            var modelName = config.LLMConfig.Model ?? "text-embedding-3-small";
            _embeddingClient = new EmbeddingClient(modelName, apiKey);

            // Konfigürasyon
            _embeddingDimension = modelName switch
            {
                "text-embedding-3-small" => 1536,
                "text-embedding-3-large" => 3072,
                "text-embedding-ada-002" => 1536,
                _ => 1536
            };

            _maxBatchSize = config.LLMConfig.MaxBatchSize;
            _enableCache = config.LLMConfig.EnableCache;
            _cacheExpiration = config.LLMConfig.CacheExpirationHours;
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new float[_embeddingDimension];
            }

            try
            {
                // Cache kontrolü
                if (_enableCache)
                {
                    var cacheKey = GenerateCacheKey(text);
                    var cachedData = (float[])CacheManager.Get(cacheKey);
                    if (cachedData != null)
                    {
                        return cachedData;
                    }
                }

                // Text preprocessing
                var processedText = PreprocessText(text);

                // OpenAI API çağrısı
                var startTime = DateTime.UtcNow;
                var embedding = await _embeddingClient.GenerateEmbeddingAsync(processedText);
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

                var embeddingArray = embedding.Value.ToFloats().ToArray();
                // Cache'e kaydet
                if (_enableCache)
                {
                    var cacheKey = GenerateCacheKey(text);
                    CacheManager.Add(cacheKey, embeddingArray, _cacheExpiration * 60);
                }

                return embeddingArray;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<float[]>> GenerateEmbeddingsAsync(List<string> texts)
        {
            if (texts == null || !texts.Any())
            {
                return new List<float[]>();
            }

            try
            {
                var results = new List<float[]>();
                var uncachedTexts = new List<(int Index, string Text)>();
                var cachedResults = new ConcurrentDictionary<int, float[]>();

                // Cache kontrolü
                if (_enableCache)
                {
                    for (int i = 0; i < texts.Count; i++)
                    {
                        var cacheKey = GenerateCacheKey(texts[i]);
                        var cachedEmbedding = (float[])CacheManager.Get(cacheKey);
                        if (cachedEmbedding != null)
                        {
                            cachedResults[i] = cachedEmbedding;
                        }
                        else
                        {
                            uncachedTexts.Add((i, texts[i]));
                        }
                    }
                }
                else
                {
                    uncachedTexts = texts.Select((text, index) => (index, text)).ToList();
                }

                // Batch işleme
                if (uncachedTexts.Any())
                {
                    var batches = SplitIntoBatches(uncachedTexts, _maxBatchSize);

                    foreach (var batch in batches)
                    {
                        var batchTexts = batch.Select(x => PreprocessText(x.Text)).ToList();
                        var startTime = DateTime.UtcNow;

                        var embeddings = await _embeddingClient.GenerateEmbeddingsAsync(batchTexts);
                        var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

                        for (int i = 0; i < batch.Count; i++)
                        {
                            var embeddingArray = embeddings.Value[i].ToFloats().ToArray();
                            var originalIndex = batch[i].Index;
                            cachedResults[originalIndex] = embeddingArray;

                            // Cache'e kaydet
                            if (_enableCache)
                            {
                                var cacheKey = GenerateCacheKey(texts[originalIndex]);
                                CacheManager.Add(cacheKey, embeddingArray, _cacheExpiration * 60);
                            }
                        }

                        // Rate limiting için kısa bekleme
                        if (batches.Count > 1)
                        {
                            await Task.Delay(100);
                        }
                    }
                }

                // Sonuçları sıralı şekilde döndür
                for (int i = 0; i < texts.Count; i++)
                {
                    results.Add(cachedResults[i]);
                }

                return results;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<EmbeddingResult>> GenerateEmbeddingsWithMetadataAsync(List<string> texts)
        {
            if (texts == null || !texts.Any())
            {
                return new List<EmbeddingResult>();
            }

            try
            {
                var results = new List<EmbeddingResult>();
                var processedTexts = texts.Select(PreprocessText).ToList();

                var batches = SplitIntoBatches(
                    processedTexts.Select((text, index) => (index, text)).ToList(),
                    _maxBatchSize
                );

                foreach (var batch in batches)
                {
                    var batchTexts = batch.Select(x => x.text).ToList();
                    var embeddings = await _embeddingClient.GenerateEmbeddingsAsync(batchTexts);

                    for (int i = 0; i < batch.Count; i++)
                    {
                        var originalIndex = batch[i].index;
                        results.Add(new EmbeddingResult
                        {
                            Embedding = embeddings.Value[i].ToFloats().ToArray(),
                            Text = texts[originalIndex],
                            TokenCount = embeddings.Value[i].Index,
                            Index = originalIndex
                        });
                    }

                    if (batches.Count > 1)
                    {
                        await Task.Delay(100);
                    }
                }

                // Orijinal sıraya göre sırala
                results = results.OrderBy(r => r.Index).ToList();
                return results;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int GetEmbeddingDimension()
        {
            return _embeddingDimension;
        }

        // Helper methods
        private string PreprocessText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // Fazla boşlukları temizle
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");

            // Trim
            text = text.Trim();

            // Token limiti kontrolü (8191 token max)
            const int maxLength = 8000; // Güvenli limit
            if (text.Length > maxLength)
            {
                text = text.Substring(0, maxLength);
            }

            return text;
        }

        private static string GenerateCacheKey(string text)
        {
            using var sha256 = SHA256.Create();
            var textBytes = Encoding.UTF8.GetBytes(text);
            var hashBytes = sha256.ComputeHash(textBytes);
            return $"embedding:{Convert.ToBase64String(hashBytes)}";
        }

        private static List<List<T>> SplitIntoBatches<T>(List<T> items, int batchSize)
        {
            var batches = new List<List<T>>();

            for (int i = 0; i < items.Count; i += batchSize)
            {
                batches.Add(items.Skip(i).Take(batchSize).ToList());
            }

            return batches;
        }
    }
}
