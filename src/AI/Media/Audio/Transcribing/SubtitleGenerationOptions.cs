using System;
using System.Collections.Generic;
using System.Text;

namespace Ophelia.AI.Media.Audio.Transcribing
{
    public class SubtitleGenerationOptions
    {
        public string OutputPath { get; set; }
        public string Model { get; set; } = "base";
        public string Language { get; set; }
        public string OutputFormat { get; set; } = "srt";
        public Dictionary<string, string> AdditionalParameters { get; set; } = new Dictionary<string, string>();
    }
}
