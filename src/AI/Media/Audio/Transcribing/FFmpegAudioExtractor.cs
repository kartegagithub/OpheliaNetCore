using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.AI.Media.Audio.Transcribing
{
    /// <summary>
    /// FFmpeg kullanarak ses çıkarma implementasyonu
    /// </summary>
    public class FFmpegAudioExtractor : IAudioExtractor
    {
        private readonly string _ffmpegPath;
        private readonly string _tempDirectory;

        public FFmpegAudioExtractor(string ffmpegPath = "ffmpeg", string tempDirectory = null)
        {
            _ffmpegPath = ffmpegPath;
            _tempDirectory = tempDirectory ?? Path.Combine(Path.GetTempPath(), "SubtitleLib");

            if (!Directory.Exists(_tempDirectory))
                Directory.CreateDirectory(_tempDirectory);
        }

        public async Task<string> ExtractAudioAsync(string inputFile, AudioExtractionOptions options)
        {
            if (!File.Exists(inputFile))
                throw new FileNotFoundException("Giriş dosyası bulunamadı", inputFile);

            var audioFile = options.OutputPath ??
                Path.Combine(_tempDirectory, $"{Guid.NewGuid()}.{options.Format}");

            var args = $"-i \"{inputFile}\" -vn -acodec {options.Codec} " +
                      $"-ar {options.SampleRate} -ac {options.Channels} \"{audioFile}\"";

            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = _ffmpegPath,
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
                throw new Exception($"FFmpeg hatası: {error}");
            }

            return audioFile;
        }
    }
}
