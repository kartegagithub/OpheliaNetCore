using System;
using System.Collections.Generic;
using System.Text;

namespace Ophelia.AI.Media.Audio.Transcribing
{
    public class AudioExtractionOptions
    {
        public string OutputPath { get; set; }
        public string Codec { get; set; } = "pcm_s16le";
        public int SampleRate { get; set; } = 16000;
        public int Channels { get; set; } = 1;
        public string Format { get; set; } = "wav";
    }
}
