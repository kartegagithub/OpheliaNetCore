using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Ophelia.AI.Models;

namespace Ophelia.AI.Extensions
{
    public static class ModelDownloader
    {
        private const string ModelUrl = "https://huggingface.co/Xenova/all-MiniLM-L6-v2/resolve/main/onnx/model.onnx";
        private const string VocabUrl = "https://huggingface.co/Xenova/all-MiniLM-L6-v2/resolve/main/vocab.txt";
        private const string DefaultDownloadDirName = "Models/Local";

        public static bool AreModelsDownloaded(AIConfig config)
        {
            if (config.LLMConfig == null || !config.LLMConfig.UseLocalEmbeding) return false;
            
            // 1. Determine Default Download Directory (relative to application base)
            string baseDir = AppContext.BaseDirectory;
            string downloadDir = Path.Combine(baseDir, DefaultDownloadDirName);

            // 2. Check and Download Model
            if (string.IsNullOrEmpty(config.LLMConfig.LocalModelPath) || !File.Exists(config.LLMConfig.LocalModelPath))
            {
                config.LLMConfig.LocalModelPath = Path.Combine(downloadDir, "model.onnx");
                config.LLMConfig.TokenizerPath = Path.Combine(downloadDir, "vocab.txt");
            }
            return File.Exists(config.LLMConfig.LocalModelPath) && File.Exists(config.LLMConfig.TokenizerPath);
        }
        /// <summary>
        /// Checks if Local Embedding is enabled. If so, ensures models are downloaded and updates the configuration paths.
        /// </summary>
        /// <param name="config">The AI configuration object.</param>
        public static async Task EnsureLocalModelsAsync(AIConfig config)
        {
            if (config?.LLMConfig == null || !config.LLMConfig.UseLocalEmbeding)
                return;

            // 1. Determine Default Download Directory (relative to application base)
            string baseDir = AppContext.BaseDirectory;
            string downloadDir = Path.Combine(baseDir, DefaultDownloadDirName);

            // 2. Check and Download Model
            if (string.IsNullOrEmpty(config.LLMConfig.LocalModelPath) || !File.Exists(config.LLMConfig.LocalModelPath))
            {
                string targetModelPath = Path.Combine(downloadDir, "model.onnx");
                if (!File.Exists(targetModelPath))
                {
                    await DownloadFileAsync(ModelUrl, targetModelPath);
                }
                config.LLMConfig.LocalModelPath = targetModelPath;
            }

            // 3. Check and Download Tokenizer
            if (string.IsNullOrEmpty(config.LLMConfig.TokenizerPath) || !File.Exists(config.LLMConfig.TokenizerPath))
            {
                string targetVocabPath = Path.Combine(downloadDir, "vocab.txt");
                if (!File.Exists(targetVocabPath))
                {
                    await DownloadFileAsync(VocabUrl, targetVocabPath);
                }
                config.LLMConfig.TokenizerPath = targetVocabPath;
            }
        }

        private static async Task DownloadFileAsync(string url, string outputPath)
        {
            var dir = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            Console.WriteLine($"Downloading {Path.GetFileName(outputPath)} from {url}...");

            using var client = new HttpClient();
            // Use ResponseHeadersRead to avoid buffering the entire file into memory
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await stream.CopyToAsync(fileStream);

            Console.WriteLine($"Downloaded: {outputPath}");
        }
    }
}
