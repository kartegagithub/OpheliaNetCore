using ImageMagick;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Ophelia.Drawing
{
    public static class ImageResizer
    {
        public static Image Rotate(Image originalImage)
        {
            try
            {
                if (originalImage != null && originalImage.PropertyIdList != null && originalImage.PropertyIdList.Contains(0x0112))
                {
                    int rotationValue = originalImage.GetPropertyItem(0x0112).Value[0];
                    switch (rotationValue)
                    {
                        case 1: // landscape, do nothing
                            break;
                        case 8: // rotated 90 right
                            originalImage.RotateFlip(rotateFlipType: RotateFlipType.Rotate270FlipNone);
                            break;
                        case 3: // bottoms up
                            originalImage.RotateFlip(rotateFlipType: RotateFlipType.Rotate180FlipNone);
                            break;
                        case 6: // rotated 90 left
                            originalImage.RotateFlip(rotateFlipType: RotateFlipType.Rotate90FlipNone);
                            break;
                    }
                }
            }
            catch (Exception)
            {
                return originalImage;
            }
            return originalImage;
        }
        public static bool IsImageFile(this string fileName)
        {
            var extension = new string[] { "png", "jpg", "jpeg", "bmp", "gif", "tiff" };
            var ext = Path.GetExtension(fileName).ToLower().Replace(".", "").Replace("ı", "i");
            return extension.Where(op => op == ext).Any();
        }
        public static Bitmap CropImage(byte[] data, int Width, int Height)
        {
            return CropImage(Bitmap.FromStream(new System.IO.MemoryStream(data)), Width, Height);
        }

        public static Bitmap CropImage(Image image, int Width, int Height)
        {
            if ((image != null))
            {
                if (image.Width > Width || image.Height > Height)
                {
                    decimal Ratio = Convert.ToDecimal(image.Width) / Convert.ToDecimal(image.Height);
                    int SizedWidth = image.Width;
                    int SizedHeight = image.Height;
                    if (Ratio > 1)
                    {
                        if (Height > image.Height)
                            Height = image.Height;
                        SizedHeight = Height;
                        SizedWidth = Convert.ToInt32(SizedHeight * Ratio);
                    }
                    else
                    {
                        if (Width > image.Width)
                            Width = image.Width;
                        SizedWidth = Width;
                        SizedHeight = Convert.ToInt32(SizedWidth / Ratio);
                    }

                    var NewImage = ProcessImage(image.ToByteArray(), SizedWidth, SizedHeight);
                    var CroppedRect = new Rectangle((NewImage.Width - Width) / 2, (NewImage.Height - Height) / 2, Width, Height);
                    var BMP = NewImage.Clone(CroppedRect, NewImage.PixelFormat);
                    return BMP;
                }
                else
                {
                    return ProcessImage(image.ToByteArray());
                }
            }
            return null;
        }
        public static void Compress(string folderPath)
        {
            var files = Directory.GetFiles(folderPath);
            foreach (var file in files)
            {
                if (!file.IsImageFile())
                    continue;
                LosslessCompress(file);
            }

            var subfolders = Directory.GetDirectories(folderPath);
            foreach (var folder in subfolders)
            {
                Compress(folder);
            }
        }
        public static Bitmap ProcessImage(byte[] data, int width = 0, int height = 0, int quality = 75, ImageFormat format = ImageFormat.Invalid)
        {
            if (format == ImageFormat.Invalid)
            {
                format = data.GetImageFormat();
                if (format == ImageFormat.Unknown)
                    format = ImageFormat.Invalid;
            }
            using (var mImage = new MagickImage(data))
            {
                if (mImage.Width > width && width > 0)
                    height = width * mImage.Height / mImage.Width;
                else if (mImage.Height > height && height > 0)
                    width = height * mImage.Width / mImage.Height;
                else
                {
                    width = mImage.Width;
                    height = mImage.Height;
                }

                if (format == ImageFormat.PNG)
                    mImage.Format = MagickFormat.Png24;
                else if (format == ImageFormat.JPEG)
                    mImage.Format = MagickFormat.Jpg;
                else if (format == ImageFormat.GIF)
                    mImage.Format = MagickFormat.Gif;
                else if (format == ImageFormat.TIFF)
                    mImage.Format = MagickFormat.Tiff;
                else if (format == ImageFormat.BMP)
                    mImage.Format = MagickFormat.Bmp;

                mImage.Quality = quality;
                if (mImage.Width != width && mImage.Height != height && width > 0 && height > 0)
                    mImage.Resize(width, height);

                return Rotate(LosslessCompress(mImage.ToByteArray())) as Bitmap;
            }
        }

        public static bool LosslessCompress(string path, bool optimalCompression = false)
        {
            try
            {
                if (!path.IsImageFile())
                    return false;
                ImageOptimizer optimizer = new ImageOptimizer();
                optimizer.OptimalCompression = optimalCompression;
                return optimizer.LosslessCompress(path);
            }
            catch (Exception)
            {
                return false;
            }            
        }
        public static Stream LosslessCompress(Stream stream, bool optimalCompression = false)
        {
            ImageOptimizer optimizer = new ImageOptimizer();
            optimizer.OptimalCompression = optimalCompression;
            optimizer.LosslessCompress(stream);
            return stream;
        }
        public static Bitmap LosslessCompress(byte[] data, bool optimalCompression = false)
        {
            var stream = new MemoryStream(data);
            ImageOptimizer optimizer = new ImageOptimizer();
            optimizer.OptimalCompression = optimalCompression;
            optimizer.LosslessCompress(stream);
            return Bitmap.FromStream(stream) as Bitmap;
        }
        
        public static Image ResizeImage(Image BMP, int width, int height, string PathToSave, int quality = 75)
        {
            try
            {
                var NewBMP = ProcessImage(BMP.ToByteArray(), width, height, quality);
                if (!string.IsNullOrEmpty(PathToSave))
                    NewBMP.Save(PathToSave);
                return NewBMP;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static void ResizeImage(Stream inputStream, int width, int height, string PathToSave, int quality = 75)
        {
            try
            {
                var NewBMP = ProcessImage(inputStream.ReadFully(0), width, height, quality);
                NewBMP.Save(PathToSave);
            }
            catch (Exception)
            {
                PathToSave = "";
            }
        }
        public static Bitmap ResizeImage(byte[] data, int width, int height, int quality = 75)
        {
            try
            {
                var NewBMP = ProcessImage(data, width, height, quality);
                return NewBMP;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
