using Ophelia.AI.Interfaces;
using Ophelia.AI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ophelia.AI.EmbeddingServices
{
    // Gemini Embedding Service
    public class GeminiEmbeddingService : IDisposable, IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly int _embeddingDimension;
        private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta";

        // Gemini embedding modelleri
        public static class Models
        {
            public const string TextEmbedding004 = "text-embedding-004";
            public const string TextEmbedding = "embedding-001";
        }

        public GeminiEmbeddingService(string apiKey, string model = Models.TextEmbedding004)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _model = model;
            _httpClient = new HttpClient();

            // Model'e göre embedding dimension ayarla
            _embeddingDimension = model switch
            {
                Models.TextEmbedding004 => 768,
                Models.TextEmbedding => 768,
                _ => 768 // Default
            };
        }

        public int GetEmbeddingDimension() => _embeddingDimension;

        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Text cannot be null or empty", nameof(text));

            try
            {
                var url = $"{BaseUrl}/models/{_model}:embedContent?key={_apiKey}";

                var requestBody = new
                {
                    content = new
                    {
                        parts = new[]
                        {
                            new { text = text }
                        }
                    },
                    taskType = "RETRIEVAL_DOCUMENT", // veya "RETRIEVAL_QUERY", "SEMANTIC_SIMILARITY"
                    title = "Document" // Opsiyonel
                };

                var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Gemini API error: {response.StatusCode} - {errorContent}");
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var responseData = JsonSerializer.Deserialize<JsonElement>(responseJson);

                if (responseData.TryGetProperty("embedding", out var embeddingElement) &&
                    embeddingElement.TryGetProperty("values", out var valuesElement))
                {
                    var values = valuesElement.EnumerateArray()
                        .Select(x => x.GetSingle())
                        .ToArray();

                    return values;
                }

                throw new InvalidOperationException("Unexpected response format from Gemini API");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Gemini embedding generation failed: {ex.Message}", ex);
            }
        }

        public async Task<List<float[]>> GenerateEmbeddingsAsync(List<string> texts)
        {
            if (texts == null || !texts.Any())
                return new List<float[]>();

            // Gemini API batch limit kontrolü
            const int batchSize = 100; // Gemini'nin batch limiti
            var results = new List<float[]>();

            for (int i = 0; i < texts.Count; i += batchSize)
            {
                var batch = texts.Skip(i).Take(batchSize).ToList();
                var batchResults = await GenerateEmbeddingsBatchAsync(batch);
                results.AddRange(batchResults);

                // Rate limiting
                if (i + batchSize < texts.Count)
                {
                    await Task.Delay(100);
                }
            }

            return results;
        }

        private async Task<List<float[]>> GenerateEmbeddingsBatchAsync(List<string> texts)
        {
            try
            {
                var url = $"{BaseUrl}/models/{_model}:batchEmbedContents?key={_apiKey}";

                var requests = texts.Select(text => new
                {
                    content = new
                    {
                        parts = new[]
                        {
                            new { text = text }
                        }
                    },
                    taskType = "RETRIEVAL_DOCUMENT"
                }).ToArray();

                var requestBody = new { requests = requests };

                var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Gemini batch API error: {response.StatusCode} - {errorContent}");
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var responseData = JsonSerializer.Deserialize<JsonElement>(responseJson);

                var results = new List<float[]>();

                if (responseData.TryGetProperty("embeddings", out var embeddingsElement))
                {
                    foreach (var embedding in embeddingsElement.EnumerateArray())
                    {
                        if (embedding.TryGetProperty("values", out var valuesElement))
                        {
                            var values = valuesElement.EnumerateArray()
                                .Select(x => x.GetSingle())
                                .ToArray();

                            results.Add(values);
                        }
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                // Fallback: tek tek embedding oluştur
                Console.WriteLine($"Gemini batch embedding failed, falling back to individual requests: {ex.Message}");
                var results = new List<float[]>();

                foreach (var text in texts)
                {
                    try
                    {
                        var embedding = await GenerateEmbeddingAsync(text);
                        results.Add(embedding);
                        await Task.Delay(50); // Rate limiting
                    }
                    catch (Exception individualEx)
                    {
                        Console.WriteLine($"Individual embedding failed for text: {individualEx.Message}");
                        // Boş embedding ekle veya skip et
                        results.Add(new float[_embeddingDimension]);
                    }
                }

                return results;
            }
        }

        public async Task<List<EmbeddingResult>> GenerateEmbeddingsWithMetadataAsync(List<string> texts)
        {
            var results = new List<EmbeddingResult>();

            for (int i = 0; i < texts.Count; i++)
            {
                try
                {
                    var embedding = await GenerateEmbeddingAsync(texts[i]);

                    results.Add(new EmbeddingResult
                    {
                        Embedding = embedding,
                        Text = texts[i],
                        Index = i,
                        TokenCount = EstimateTokenCount(texts[i])
                    });
                }
                catch (Exception)
                {
                    results.Add(new EmbeddingResult
                    {
                        Embedding = new float[_embeddingDimension],
                        Text = texts[i],
                        Index = i
                    });
                }

                // Rate limiting
                if (i < texts.Count - 1)
                {
                    await Task.Delay(50);
                }
            }

            return results;
        }

        private static int EstimateTokenCount(string text)
        {
            // Basit token estimation (gerçek tokenizer kullanılabilir)
            return (int)Math.Ceiling(text.Length / 4.0);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}