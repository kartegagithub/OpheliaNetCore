using System;
using System.Collections.Generic;
using System.Text;

namespace Ophelia.AI.Media.Audio.Transcribing
{
    /// <summary>
    /// SRT dosya işlemlerini tanımlar
    /// </summary>
    public interface ISrtFileHandler
    {
        List<SubtitleEntry> ParseSrtFile(string srtFile);
        void WriteSrtFile(string outputPath, List<SubtitleEntry> subtitles);
    }

}
