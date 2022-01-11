using System;
using System.Drawing;
using System.IO;

namespace Ophelia
{
    public static class BitmapExtensions
    {
        public static Bitmap Rotate(this Bitmap source, float degree)
        {
            double angle = Math.PI * Math.Abs(degree) / 180.0;
            int width = (int)((Math.Sin(angle) * source.Height) + (Math.Cos(angle) * source.Width));
            int height = (int)((Math.Sin(angle) * source.Width) + (Math.Cos(angle) * source.Height));
            var rotatedImage = new Bitmap(Math.Abs(width), Math.Abs(height));
            using (Graphics graph = Graphics.FromImage(rotatedImage))
            {
                graph.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graph.TranslateTransform(rotatedImage.Width / 2, rotatedImage.Height / 2);
                graph.RotateTransform(degree);
                graph.TranslateTransform(-source.Width / 2, -source.Height / 2);
                graph.DrawImageUnscaled(source, Point.Empty);
            }
            return rotatedImage;
        }
        public static Bitmap AddFrame(this Bitmap source, Color color, int frameWidth)
        {
            Bitmap frameImage = new Bitmap(source.Width + frameWidth * 2, source.Height + frameWidth * 2);
            using (Graphics graph = Graphics.FromImage(frameImage))
            {
                int x = (frameImage.Width - source.Width) / 2;
                int y = (frameImage.Height - source.Height) / 2;
                graph.Clear(color);
                graph.DrawImage(source, new Point(x, y));
            }
            return frameImage;
        }
        public static Bitmap AddFrame(this Bitmap source, Color color, int leftWidth, int rightWidth, int topWidth, int bottomWidth)
        {
            Bitmap frameImage = new Bitmap(source.Width + leftWidth + rightWidth, source.Height + topWidth + bottomWidth);
            using (Graphics graph = Graphics.FromImage(frameImage))
            {
                int x = leftWidth;
                int y = topWidth;
                graph.Clear(color);
                graph.DrawImage(source, new Point(x, y));
            }
            return frameImage;
        }
        public static Bitmap Combine(this Bitmap source, Bitmap image, int x, int y, int width, int height)
        {
            using (Graphics graph = Graphics.FromImage(source))
            {
                graph.DrawImage(image, x, y, width, height);
            }
            return source;
        }
        public static Bitmap Combine(this Bitmap source, Bitmap image, Point point, Size size)
        {
            using (Graphics graph = Graphics.FromImage(source))
            {
                if (size.Width <= 0)
                    graph.DrawImage(image, point);
                else
                    graph.DrawImage(image, point.X, point.Y, size.Width, size.Height);
            }
            return source;
        }
        public static byte[] ToByteArray(this Bitmap source, System.Drawing.Imaging.ImageFormat format)
        {
            using (var stream = new MemoryStream())
            {
                source.Save(stream, format);
                return stream.ToArray();
            }
        }
    }
}
