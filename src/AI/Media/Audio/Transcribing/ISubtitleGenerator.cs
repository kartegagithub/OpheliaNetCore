using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.AI.Media.Audio.Transcribing
{
    /// <summary>
    /// Altyazı oluşturma işlemlerini tanımlar
    /// </summary>
    public interface ISubtitleGenerator
    {
        Task<string> GenerateSubtitlesAsync(string audioFile, SubtitleGenerationOptions options);
        string GeneratorName { get; }
    }
}
