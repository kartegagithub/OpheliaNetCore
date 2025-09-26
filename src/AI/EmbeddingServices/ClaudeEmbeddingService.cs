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
    public class ClaudeEmbeddingService : IDisposable, IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly int _embeddingDimension;
        private const string BaseUrl = "https://api.anthropic.com/v1";

        // Not: Claude'un native embedding API'si yoktur
        // Bu implementation text'i Claude ile process edip synthetic embedding oluşturur
        // Gerçek production için OpenAI, Cohere vs. kullanılmalı

        public static class Models
        {
            public const string Claude3Sonnet = "claude-3-sonnet-20240229";
            public const string Claude3Haiku = "claude-3-haiku-20240307";
        }

        public ClaudeEmbeddingService(string apiKey, string model = Models.Claude3Haiku, int dimension = 384)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _model = model;
            _embeddingDimension = dimension;

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        }

        public int GetEmbeddingDimension() => _embeddingDimension;

        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Text cannot be null or empty", nameof(text));

            try
            {
                // Claude ile semantic analysis yapıp synthetic embedding oluştur
                var prompt = $@"Analyze the following text and provide {_embeddingDimension} semantic feature scores between -1 and 1, representing various semantic dimensions like sentiment, topic, complexity, etc. Return only comma-separated numbers:
                Text: {text}
                Semantic scores:";

                var requestBody = new
                {
                    model = _model,
                    max_tokens = 1000,
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = prompt
                        }
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{BaseUrl}/messages", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Claude API error: {response.StatusCode} - {errorContent}");
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var responseData = JsonSerializer.Deserialize<JsonElement>(responseJson);

                if (responseData.TryGetProperty("content", out var contentArray) &&
                    contentArray.GetArrayLength() > 0)
                {
                    var textContent = contentArray[0].GetProperty("text").GetString();

                    // Response'tan sayıları parse et
                    var numbers = textContent.Split(',')
                        .Select(s => s.Trim())
                        .Where(s => float.TryParse(s, out _))
                        .Select(float.Parse)
                        .ToArray();

                    if (numbers.Length >= _embeddingDimension)
                    {
                        return numbers.Take(_embeddingDimension).ToArray();
                    }
                    else
                    {
                        // Eksik boyutları random ile doldur
                        var result = new float[_embeddingDimension];
                        Array.Copy(numbers, result, Math.Min(numbers.Length, _embeddingDimension));

                        var random = new Random(text.GetHashCode()); // Deterministic
                        for (int i = numbers.Length; i < _embeddingDimension; i++)
                        {
                            result[i] = (float)(random.NextDouble() * 2 - 1); // -1 to 1
                        }

                        return result;
                    }
                }

                // Fallback: Hash-based synthetic embedding
                return GenerateHashBasedEmbedding(text);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Claude embedding generation failed, using fallback: {ex.Message}");
                return GenerateHashBasedEmbedding(text);
            }
        }

        private float[] GenerateHashBasedEmbedding(string text)
        {
            // Text'in hash'ini kullanarak deterministic synthetic embedding oluştur
            var hash = text.GetHashCode();
            var random = new Random(hash);

            var embedding = new float[_embeddingDimension];
            for (int i = 0; i < _embeddingDimension; i++)
            {
                // Text karakteristiklerine göre weighted random values
                var charWeight = (i < text.Length) ? text[i] / 255f : 0.5f;
                var randomComponent = (float)(random.NextDouble() * 2 - 1);

                embedding[i] = (charWeight + randomComponent) / 2f;
            }

            // Normalize to unit vector
            var magnitude = (float)Math.Sqrt(embedding.Sum(x => x * x));
            if (magnitude > 0)
            {
                for (int i = 0; i < embedding.Length; i++)
                {
                    embedding[i] /= magnitude;
                }
            }

            return embedding;
        }

        public async Task<List<float[]>> GenerateEmbeddingsAsync(List<string> texts)
        {
            if (texts == null || !texts.Any())
                return new List<float[]>();

            var results = new List<float[]>();

            // Claude API rate limits nedeniyle tek tek işle
            foreach (var text in texts)
            {
                try
                {
                    var embedding = await GenerateEmbeddingAsync(text);
                    results.Add(embedding);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Claude embedding failed for text: {ex.Message}");
                    results.Add(GenerateHashBasedEmbedding(text));
                }

                // Rate limiting
                await Task.Delay(200); // Claude API için conservative rate limit
            }

            return results;
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
                        TokenCount = EstimateTokenCount(texts[i]),
                    });
                }
                catch (Exception)
                {
                    results.Add(new EmbeddingResult
                    {
                        Embedding = GenerateHashBasedEmbedding(texts[i]),
                        Text = texts[i],
                        Index = i,
                    });
                }

                // Rate limiting
                await Task.Delay(200);
            }

            return results;
        }

        private static int EstimateTokenCount(string text)
        {
            // Claude tokenizer estimation
            return (int)Math.Ceiling(text.Length / 3.5);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
