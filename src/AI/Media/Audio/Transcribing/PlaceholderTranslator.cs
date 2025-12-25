using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.AI.Media.Audio.Transcribing
{
    /// <summary>
    /// Placeholder çeviri implementasyonu
    /// </summary>
    public class PlaceholderTranslator : ISubtitleTranslator
    {
        public string TranslatorName => "Placeholder";

        public async Task<string> TranslateAsync(string text, string sourceLanguage, string targetLanguage)
        {
            await Task.Delay(10);
            return $"[{targetLanguage.ToUpper()}] {text}";
        }
    }
}
