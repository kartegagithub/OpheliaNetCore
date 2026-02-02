using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Ophelia.AI.Interfaces;
using Ophelia.AI.Models;
using Ophelia.Caching;

namespace Ophelia.AI.EmbeddingServices
{
    /// <summary>
    /// Microsoft.ML.OnnxRuntime kullanarak lokal embedding üretir.
    /// Gerekli Paketler: Microsoft.ML.OnnxRuntime, Microsoft.ML.Tokenizers (veya basit tokenizer)
    /// </summary>
    public class LocalOnnxEmbeddingService : IEmbeddingService, IDisposable
    {
        private readonly InferenceSession _session;
        private readonly AIConfig _config;
        private readonly bool _enableCache;
        private readonly int _cacheExpiration;
        private readonly int _embeddingDimension;

        // Basit WordPiece Tokenizer (Daha gelişmiş tokenizer için Microsoft.ML.Tokenizers kullanılmalı)
        private readonly Dictionary<string, int> _vocab;
        
        public LocalOnnxEmbeddingService(AIConfig config)
        {
            _config = config;
            _enableCache = config.LLMConfig.EnableCache;
            _cacheExpiration = config.LLMConfig.CacheExpirationHours;
            _embeddingDimension = 384;

            // Model ve Tokenizer yolları kontrolü
            if (string.IsNullOrEmpty(config.LLMConfig.LocalModelPath) || !System.IO.File.Exists(config.LLMConfig.LocalModelPath))
                throw new FileNotFoundException($"Model file not found at {config.LLMConfig.LocalModelPath}");

            if (string.IsNullOrEmpty(config.LLMConfig.TokenizerPath) || !System.IO.File.Exists(config.LLMConfig.TokenizerPath))
                throw new FileNotFoundException($"Tokenizer vocabulary file not found at {config.LLMConfig.TokenizerPath}");

            // Onnx Session başlat
            // CPU kullanımı için varsayılan session options yeterli
            // GPU için: SessionOptions.MakeSessionOptionWithCudaProvider()
            _session = new InferenceSession(config.LLMConfig.LocalModelPath);

            // Vocab dosyasını yükle (basit format: her satırda bir token)
            _vocab = LoadVocab(config.LLMConfig.TokenizerPath);
        }

        public void Dispose()
        {
            _session?.Dispose();
            GC.SuppressFinalize(this);
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

        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
             if (string.IsNullOrWhiteSpace(text))
                return new float[GetEmbeddingDimension()];

            if (_enableCache)
            {
                var cacheKey = GenerateCacheKey(text);
                var cachedData = (float[])CacheManager.Get(cacheKey);
                if (cachedData != null) return cachedData;
            }

            return await Task.Run(() =>
            {
                var embedding = RunInference(text);
                
                if (_enableCache)
                {
                    var cacheKey = GenerateCacheKey(text);
                    CacheManager.Add(cacheKey, embedding, _cacheExpiration * 60);
                }
                return embedding;
            });
        }

        public async Task<List<float[]>> GenerateEmbeddingsAsync(List<string> texts)
        {
            var results = new List<float[]>();
            foreach (var text in texts)
            {
                results.Add(await GenerateEmbeddingAsync(text));
            }
            return results;
        }

        private float[] RunInference(string text)
        {
            // 1. Tokenize
            var tokens = SimpleTokenize(text);
            var inputIds = tokens.Select(t => (long)_vocab.GetValueOrDefault(t, _vocab.GetValueOrDefault("[UNK]", 100))).ToArray();
            
            // Padding / Truncating (Modelin max uzunluğuna göre, genelde 512)
            int maxLength = 512;
            if (inputIds.Length > maxLength - 2) // [CLS] ve [SEP] için yer ayır
            {
                inputIds = inputIds.Take(maxLength - 2).ToArray();
            }

            // [CLS] + inputIds + [SEP]
            var inputList = new List<long> { _vocab.GetValueOrDefault("[CLS]", 101) };
            inputList.AddRange(inputIds);
            inputList.Add(_vocab.GetValueOrDefault("[SEP]", 102));

            long[] finalInputIds = inputList.ToArray();
            long[] attentionMask = Enumerable.Repeat(1L, finalInputIds.Length).ToArray();
            long[] tokenTypeIds = new long[finalInputIds.Length]; // Hepsi 0 (tek cümle)

            // 2. Tensors oluştur
            var inputIdsTensor = new DenseTensor<long>(finalInputIds, new[] { 1, finalInputIds.Length });
            var attentionMaskTensor = new DenseTensor<long>(attentionMask, new[] { 1, attentionMask.Length });
            var tokenTypeIdsTensor = new DenseTensor<long>(tokenTypeIds, new[] { 1, tokenTypeIds.Length });

            // 3. Inputs hazırla
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input_ids", inputIdsTensor),
                NamedOnnxValue.CreateFromTensor("attention_mask", attentionMaskTensor),
                NamedOnnxValue.CreateFromTensor("token_type_ids", tokenTypeIdsTensor)
            };

            // 4. Run Inference
            using var results = _session.Run(inputs);

            // 5. Output al (Genelde 'last_hidden_state' veya 'pooler_output')
            // all-MiniLM-L6-v2 gibi modellerde ortalama (mean pooling) almak gerekebilir
            // Ancak basitleştirilmiş olarak [CLS] token embedding'ini alabiliriz.
            // Daha doğru sonuç için Mean Pooling önerilir.
            
            var output = results.First().AsTensor<float>();
            // [Batch, SequenceLength, HiddenSize] => [1, SeqLen, 384]
            
            // Mean Pooling Implementasyonu
            int hiddenSize = output.Dimensions[2];
            int seqLen = output.Dimensions[1];
            
            var pooledEmbedding = new float[hiddenSize];
            
            for (int h = 0; h < hiddenSize; h++)
            {
                float sum = 0;
                for (int s = 0; s < seqLen; s++)
                {
                    // Attention mask dikkate alınmalı
                    sum += output[0, s, h]; 
                }
                pooledEmbedding[h] = sum / seqLen;
            }

            // Normalize (Cosine Similarity için önemli)
            return Normalize(pooledEmbedding);
        }

        private float[] Normalize(float[] vector)
        {
            double sumSq = 0;
            foreach (var val in vector) sumSq += val * val;
            float norm = (float)Math.Sqrt(sumSq);
            
            if (norm == 0) return vector;

            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] /= norm;
            }
            return vector;
        }

        // Basitkelime tokenizasyonu (Gerçek WordPiece değil ama idare eder)
        private List<string> SimpleTokenize(string text)
        {
            // Küçük harfe çevir ve noktalama işaretlerini ayır
            text = text.ToLowerInvariant();
            // Basit split (gelişmiş tokenizer şart aslında)
            return text.Split(new[] { ' ', '.', ',', '!', '?', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public int EstimateTokenCount(string text)
        {
            var tokens = SimpleTokenize(text);
            return tokens.Count;
        }

        private Dictionary<string, int> LoadVocab(string path)
        {
            var vocab = new Dictionary<string, int>();
            var lines = System.IO.File.ReadAllLines(path);
            for (int i = 0; i < lines.Length; i++)
            {
                if (!vocab.ContainsKey(lines[i]))
                    vocab.Add(lines[i], i);
            }
            return vocab;
        }

        private static string GenerateCacheKey(string text)
        {
            using var sha256 = SHA256.Create();
            var textBytes = Encoding.UTF8.GetBytes(text);
            var hashBytes = sha256.ComputeHash(textBytes);
            return $"local_embedding:{Convert.ToBase64String(hashBytes)}";
        }
        
        public int GetEmbeddingDimension()
        {
            return _config.VectorConfig.Dimension > 0 ? _config.VectorConfig.Dimension : 384; 
        }
    }
}
