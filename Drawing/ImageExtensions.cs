using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ophelia
{
    public enum ImageFormat
    {
        BMP,
        JPEG,
        GIF,
        TIFF,
        PNG,
        Unknown,
        Invalid
    }

    public static class ImageExtensions
    {

        public static bool IsAcceptableImage(this byte[] bytes, params ImageFormat[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
                return false;
            return parameters.Contains(bytes.GetImageFormat());
        }

        /// <summary>
        /// https://stackoverflow.com/a/9446045/1766100
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static ImageFormat GetImageFormat(this byte[] bytes)
        {
            if (bytes == null)
                return ImageFormat.Invalid;

            // see http://www.mikekunz.com/image_file_header.html  
            var bmp = Encoding.ASCII.GetBytes("BM");     // BMP
            var gif = Encoding.ASCII.GetBytes("GIF");    // GIF
            var png = new byte[] { 137, 80, 78, 71 };    // PNG
            var tiff = new byte[] { 73, 73, 42 };         // TIFF
            var tiff2 = new byte[] { 77, 77, 42 };         // TIFF
            var jpeg = new byte[] { 255, 216, 255, 224 }; // jpeg
            var jpeg2 = new byte[] { 255, 216, 255, 225 }; // jpeg canon
            var jpeg3 = new byte[] { 255, 216, 255, 237 };

            if (bmp.SequenceEqual(bytes.Take(bmp.Length)))
                return ImageFormat.BMP;

            if (gif.SequenceEqual(bytes.Take(gif.Length)))
                return ImageFormat.GIF;

            if (png.SequenceEqual(bytes.Take(png.Length)))
                return ImageFormat.PNG;

            if (tiff.SequenceEqual(bytes.Take(tiff.Length)))
                return ImageFormat.TIFF;

            if (tiff2.SequenceEqual(bytes.Take(tiff2.Length)))
                return ImageFormat.TIFF;

            if (jpeg.SequenceEqual(bytes.Take(jpeg.Length)))
                return ImageFormat.JPEG;

            if (jpeg2.SequenceEqual(bytes.Take(jpeg2.Length)))
                return ImageFormat.JPEG;

            if (jpeg3.SequenceEqual(bytes.Take(jpeg3.Length)))
                return ImageFormat.JPEG;

            return ImageFormat.Unknown;
        }
    }
}
