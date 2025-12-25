using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ophelia.AI.Media.Audio.Transcribing
{
    /// <summary>
    /// SRT dosya işleme implementasyonu
    /// </summary>
    public class SrtFileHandler : ISrtFileHandler
    {
        public List<SubtitleEntry> ParseSrtFile(string srtFile)
        {
            var subtitles = new List<SubtitleEntry>();
            var lines = File.ReadAllLines(srtFile, Encoding.UTF8);

            SubtitleEntry current = null;
            var textBuilder = new StringBuilder();

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    if (current != null && textBuilder.Length > 0)
                    {
                        current.Text = textBuilder.ToString().Trim();
                        subtitles.Add(current);
                        current = null;
                        textBuilder.Clear();
                    }
                    continue;
                }

                if (int.TryParse(line, out int index))
                {
                    current = new SubtitleEntry { Index = index };
                }
                else if (line.Contains("-->") && current != null)
                {
                    var times = line.Split(new[] { "-->" }, StringSplitOptions.None);
                    current.StartTime = times[0].Trim();
                    current.EndTime = times[1].Trim();
                }
                else if (current != null)
                {
                    if (textBuilder.Length > 0)
                        textBuilder.AppendLine();
                    textBuilder.Append(line);
                }
            }

            if (current != null && textBuilder.Length > 0)
            {
                current.Text = textBuilder.ToString().Trim();
                subtitles.Add(current);
            }

            return subtitles;
        }

        public void WriteSrtFile(string outputPath, List<SubtitleEntry> subtitles)
        {
            using var writer = new StreamWriter(outputPath, false, Encoding.UTF8);

            foreach (var subtitle in subtitles)
            {
                writer.WriteLine(subtitle.Index);
                writer.WriteLine($"{subtitle.StartTime} --> {subtitle.EndTime}");
                writer.WriteLine(subtitle.Text);
                writer.WriteLine();
            }
        }
    }
}
