using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.AI.Media.Audio.Transcribing
{
    /// <summary>
    /// Whisper kullanarak altyazı oluşturma implementasyonu
    /// </summary>
    public class WhisperSubtitleGenerator : ISubtitleGenerator
    {
        private readonly string _whisperPath;

        public string GeneratorName => "Whisper";

        public WhisperSubtitleGenerator(string whisperPath = "whisper")
        {
            _whisperPath = whisperPath;
        }

        public async Task<string> GenerateSubtitlesAsync(string audioFile, SubtitleGenerationOptions options)
        {
            if (!File.Exists(audioFile))
                throw new FileNotFoundException("Ses dosyası bulunamadı", audioFile);

            var outputDir = options.OutputPath != null
                ? Path.GetDirectoryName(options.OutputPath)
                : Path.GetDirectoryName(audioFile);

            var args = $"\"{audioFile}\" --model {options.Model} " +
                      $"--output_format {options.OutputFormat} --output_dir \"{outputDir}\"";

            if (!string.IsNullOrEmpty(options.Language))
                args += $" --language {options.Language}";

            foreach (var param in options.AdditionalParameters)
                args += $" --{param.Key} {param.Value}";

            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = _whisperPath,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                var error = await process.StandardError.ReadToEndAsync();
                throw new Exception($"Whisper hatası: {error}");
            }

            var srtFile = Path.Combine(outputDir,
                $"{Path.GetFileNameWithoutExtension(audioFile)}.{options.OutputFormat}");

            if (options.OutputPath != null && File.Exists(srtFile))
            {
                var targetFile = options.OutputPath.EndsWith($".{options.OutputFormat}")
                    ? options.OutputPath
                    : options.OutputPath + $".{options.OutputFormat}";
                File.Move(srtFile, targetFile);
                return targetFile;
            }

            return srtFile;
        }
    }
}
