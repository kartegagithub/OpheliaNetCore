using Ophelia.AI.Interfaces;
using Ophelia.AI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ophelia.AI.EmbeddingServices
{
    // Hugging Face Embedding Service
    public class HuggingFaceEmbeddingService : IDisposable, IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly int _embeddingDimension;
        private const string BaseUrl = "https://api-inference.huggingface.co/pipeline/feature-extraction";

        public HuggingFaceEmbeddingService(AIConfig config)
        {
            _apiKey = config.LLMConfig.APIKey;
            _model = config.LLMConfig.EmbedingModel;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);

            // Model'e göre embedding dimension ayarla
            _embeddingDimension = _model switch
            {
                "sentence-transformers/all-MiniLM-L6-v2" => 384,
                "sentence-transformers/all-mpnet-base-v2" => 768,
                "sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2" => 384,
                "BAAI/bge-small-en-v1.5" => 384,
                "BAAI/bge-base-en-v1.5" => 768,
                "BAAI/bge-large-en-v1.5" => 1024,
                "intfloat/e5-small-v2" => 384,
                "intfloat/e5-base-v2" => 768,
                "intfloat/e5-large-v2" => 1024,
                "thenlper/gte-small" => 384,
                "thenlper/gte-base" => 768,
                "thenlper/gte-large" => 1024,
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
                var url = $"{BaseUrl}/{_model}";

                var requestBody = new
                {
                    inputs = text,
                    options = new
                    {
                        wait_for_model = true
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();

                    // Model loading olabilir, tekrar dene
                    if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                    {
                        await Task.Delay(2000);
                        response = await _httpClient.PostAsync(url, content);

                        if (!response.IsSuccessStatusCode)
                        {
                            errorContent = await response.Content.ReadAsStringAsync();
                            throw new HttpRequestException($"Hugging Face API error: {response.StatusCode} - {errorContent}");
                        }
                    }
                    else
                    {
                        throw new HttpRequestException($"Hugging Face API error: {response.StatusCode} - {errorContent}");
                    }
                }

                var responseJson = await response.Content.ReadAsStringAsync();

                // Response formatı model tipine göre değişebilir
                // Genelde [[embedding]] veya [embedding] formatında gelir
                var responseData = JsonSerializer.Deserialize<JsonElement>(responseJson);

                float[] values;

                // Array of arrays kontrolü
                if (responseData.ValueKind == JsonValueKind.Array &&
                    responseData.GetArrayLength() > 0)
                {
                    var firstElement = responseData[0];

                    if (firstElement.ValueKind == JsonValueKind.Array)
                    {
                        // [[embedding]] formatı
                        values = firstElement.EnumerateArray()
                            .Select(x => x.GetSingle())
                            .ToArray();
                    }
                    else
                    {
                        // [embedding] formatı
                        values = responseData.EnumerateArray()
                            .Select(x => x.GetSingle())
                            .ToArray();
                    }
                }
                else
                {
                    throw new InvalidOperationException("Unexpected response format from Hugging Face API");
                }

                return values;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Hugging Face embedding generation failed: {ex.Message}", ex);
            }
        }

        public async Task<List<float[]>> GenerateEmbeddingsAsync(List<string> texts)
        {
            if (texts == null || !texts.Any())
                return new List<float[]>();

            // Hugging Face API batch işlemi destekler
            const int batchSize = 32; // Batch size (model ve rate limit'e göre ayarlanabilir)
            var results = new List<float[]>();

            for (int i = 0; i < texts.Count; i += batchSize)
            {
                var batch = texts.Skip(i).Take(batchSize).ToList();
                var batchResults = await GenerateEmbeddingsBatchAsync(batch);
                results.AddRange(batchResults);

                // Rate limiting
                if (i + batchSize < texts.Count)
                {
                    await Task.Delay(200);
                }
            }

            return results;
        }

        private async Task<List<float[]>> GenerateEmbeddingsBatchAsync(List<string> texts)
        {
            try
            {
                var url = $"{BaseUrl}/{_model}";

                var requestBody = new
                {
                    inputs = texts,
                    options = new
                    {
                        wait_for_model = true
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();

                    // Model loading olabilir, tekrar dene
                    if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                    {
                        await Task.Delay(3000);
                        response = await _httpClient.PostAsync(url, content);

                        if (!response.IsSuccessStatusCode)
                        {
                            throw new HttpRequestException($"Hugging Face batch API error after retry");
                        }
                    }
                    else
                    {
                        throw new HttpRequestException($"Hugging Face batch API error: {response.StatusCode} - {errorContent}");
                    }
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var responseData = JsonSerializer.Deserialize<JsonElement>(responseJson);

                var results = new List<float[]>();

                // Batch response: [[emb1], [emb2], ...]
                if (responseData.ValueKind == JsonValueKind.Array)
                {
                    foreach (var embeddingArray in responseData.EnumerateArray())
                    {
                        float[] values;

                        if (embeddingArray.ValueKind == JsonValueKind.Array)
                        {
                            // Her bir embedding array
                            if (embeddingArray.GetArrayLength() > 0 &&
                                embeddingArray[0].ValueKind == JsonValueKind.Array)
                            {
                                // Nested array [[embedding]]
                                values = embeddingArray[0].EnumerateArray()
                                    .Select(x => x.GetSingle())
                                    .ToArray();
                            }
                            else
                            {
                                // Direct array [embedding]
                                values = embeddingArray.EnumerateArray()
                                    .Select(x => x.GetSingle())
                                    .ToArray();
                            }

                            results.Add(values);
                        }
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                // Fallback: tek tek embedding oluştur
                Console.WriteLine($"Hugging Face batch embedding failed, falling back to individual requests: {ex.Message}");
                var results = new List<float[]>();

                foreach (var text in texts)
                {
                    try
                    {
                        var embedding = await GenerateEmbeddingAsync(text);
                        results.Add(embedding);
                        await Task.Delay(100); // Rate limiting
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

            // Batch işlemi için önce tüm embeddings'leri al
            var embeddings = await GenerateEmbeddingsAsync(texts);

            for (int i = 0; i < texts.Count; i++)
            {
                results.Add(new EmbeddingResult
                {
                    Embedding = i < embeddings.Count ? embeddings[i] : new float[_embeddingDimension],
                    Text = texts[i],
                    Index = i,
                    TokenCount = EstimateTokenCount(texts[i])
                });
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