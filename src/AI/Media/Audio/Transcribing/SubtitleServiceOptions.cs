using System;
using System.Collections.Generic;
using System.Text;

namespace Ophelia.AI.Media.Audio.Transcribing
{
    public class SubtitleServiceOptions
    {
        public string FFmpegPath { get; set; } = "ffmpeg";
        public string WhisperPath { get; set; } = "whisper";
        public string TempDirectory { get; set; }
    }
}
