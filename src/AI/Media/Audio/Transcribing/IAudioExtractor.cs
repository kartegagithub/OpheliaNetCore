using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.AI.Media.Audio.Transcribing
{
    /// <summary>
    /// Ses dosyası çıkarma işlemlerini tanımlar
    /// </summary>
    public interface IAudioExtractor
    {
        Task<string> ExtractAudioAsync(string inputFile, AudioExtractionOptions options);
    }
}
