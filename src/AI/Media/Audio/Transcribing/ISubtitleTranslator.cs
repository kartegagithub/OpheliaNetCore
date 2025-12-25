using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.AI.Media.Audio.Transcribing
{
    /// <summary>
    /// Altyazı çeviri işlemlerini tanımlar
    /// </summary>
    public interface ISubtitleTranslator
    {
        Task<string> TranslateAsync(string text, string sourceLanguage, string targetLanguage);
        string TranslatorName { get; }
    }
}
