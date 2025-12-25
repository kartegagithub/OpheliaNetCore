using System;
using System.Collections.Generic;
using System.Text;

namespace Ophelia.AI.Media.Audio.Transcribing
{
    public class SubtitleEntry
    {
        public int Index { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Text { get; set; }
    }
}
