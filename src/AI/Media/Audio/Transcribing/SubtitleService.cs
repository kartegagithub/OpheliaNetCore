using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.AI.Media.Audio.Transcribing
{
    /// <summary>
    /// Ana altyazı işleme servisi - Dependency Injection ile kullanılır
    /// </summary>
    public class SubtitleService
    {
        private readonly IAudioExtractor _audioExtractor;
        private readonly ISubtitleGenerator _subtitleGenerator;
        private readonly ISubtitleTranslator _translator;
        private readonly ISrtFileHandler _srtHandler;

        public SubtitleService(
            IAudioExtractor audioExtractor,
            ISubtitleGenerator subtitleGenerator,
            ISubtitleTranslator translator,
            ISrtFileHandler srtHandler)
        {
            _audioExtractor = audioExtractor ?? throw new ArgumentNullException(nameof(audioExtractor));
            _subtitleGenerator = subtitleGenerator ?? throw new ArgumentNullException(nameof(subtitleGenerator));
            _translator = translator ?? throw new ArgumentNullException(nameof(translator));
            _srtHandler = srtHandler ?? throw new ArgumentNullException(nameof(srtHandler));
        }

        /// <summary>
        /// Video/ses dosyasından altyazı oluşturur
        /// </summary>
        public async Task<string> CreateSubtitlesAsync(
            string inputFile,
            AudioExtractionOptions audioOptions = null,
            SubtitleGenerationOptions generationOptions = null)
        {
            audioOptions ??= new AudioExtractionOptions();
            generationOptions ??= new SubtitleGenerationOptions();

            // 1. Ses çıkar
            var audioFile = await _audioExtractor.ExtractAudioAsync(inputFile, audioOptions);

            try
            {
                // 2. Altyazı oluştur
                var srtFile = await _subtitleGenerator.GenerateSubtitlesAsync(audioFile, generationOptions);
                return srtFile;
            }
            finally
            {
                // Geçici ses dosyasını temizle
                if (audioOptions.OutputPath == null && File.Exists(audioFile))
                {
                    try { File.Delete(audioFile); } catch { }
                }
            }
        }

        /// <summary>
        /// SRT dosyasını başka bir dile çevirir
        /// </summary>
        public async Task<string> TranslateSubtitlesAsync(
            string srtFile,
            string targetLanguage,
            string sourceLanguage = null,
            string outputPath = null)
        {
            if (!File.Exists(srtFile))
                throw new FileNotFoundException("SRT dosyası bulunamadı", srtFile);

            var subtitles = _srtHandler.ParseSrtFile(srtFile);
            var translatedSubtitles = new List<SubtitleEntry>();

            foreach (var subtitle in subtitles)
            {
                var translatedText = await _translator.TranslateAsync(
                    subtitle.Text, sourceLanguage, targetLanguage);

                translatedSubtitles.Add(new SubtitleEntry
                {
                    Index = subtitle.Index,
                    StartTime = subtitle.StartTime,
                    EndTime = subtitle.EndTime,
                    Text = translatedText
                });
            }

            outputPath ??= Path.Combine(
                Path.GetDirectoryName(srtFile),
                $"{Path.GetFileNameWithoutExtension(srtFile)}_{targetLanguage}.srt");

            _srtHandler.WriteSrtFile(outputPath, translatedSubtitles);
            return outputPath;
        }
    }
}
