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
        public static void ProcessImages(string folderPath, int quality = 75)
        {
            var files = Directory.GetFiles(folderPath);
            foreach (var file in files)
            {
                ProcessImage(file, quality);
            }

            var subfolders = Directory.GetDirectories(folderPath);
            foreach (var folder in subfolders)
            {
                ProcessImages(folder, quality);
            }
        }
        public static Bitmap ProcessImage(string filePath, int quality = 75)
        {
            var extension = Path.GetExtension(filePath).Replace(".", "").ToLower();
            var format = ImageFormat.Unknown;
            switch (extension)
            {
                case "jpg":
                case "jpeg":
                    format = ImageFormat.JPEG;
                    break;
                case "png":
                    format = ImageFormat.PNG;
                    break;
                case "gif":
                    format = ImageFormat.GIF;
                    break;
                case "tiff":
                    format = ImageFormat.TIFF;
                    break;
            }
            if (format == ImageFormat.Unknown)
                return null;
            return ProcessImage(System.IO.File.ReadAllBytes(filePath), 0, 0, quality, format);
        }
        public static Bitmap ProcessImage(byte[] data, int width = 0, int height = 0, int quality = 75, ImageFormat format = ImageFormat.Invalid)
        {
            if (format == ImageFormat.Invalid)
            {
                format = data.GetImageFormat();
                if (format == ImageFormat.Unknown)
                    format = ImageFormat.Invalid;
            }
            var BMP = Bitmap.FromStream(new MemoryStream(data));

            Rotate(BMP);
            if (BMP.Width > width)
                height = width * BMP.Height / BMP.Width;
            else if (BMP.Height > height)
                width = height * BMP.Width / BMP.Height;
            else
            {
                width = BMP.Width;
                height = BMP.Height;
            }
            var image = new Bitmap(BMP, width, height);

            using (var mImage = new MagickImage(image.ToByteArray()))
            {
                if (format == ImageFormat.Invalid)
                {
                    if (image.RawFormat == System.Drawing.Imaging.ImageFormat.Png)
                        mImage.Format = MagickFormat.Png24;
                    else if (image.RawFormat == System.Drawing.Imaging.ImageFormat.Jpeg)
                        mImage.Format = MagickFormat.Jpg;
                    else if (image.RawFormat == System.Drawing.Imaging.ImageFormat.Gif)
                        mImage.Format = MagickFormat.Gif;
                    else if (image.RawFormat == System.Drawing.Imaging.ImageFormat.Tiff)
                        mImage.Format = MagickFormat.Tiff;
                    else if (image.RawFormat == System.Drawing.Imaging.ImageFormat.Bmp)
                        mImage.Format = MagickFormat.Bmp;
                    else if (image.RawFormat == System.Drawing.Imaging.ImageFormat.MemoryBmp || image.RawFormat.ToString() == "MemoryBMP")
                        mImage.Format = MagickFormat.Bmp;
                }
                else
                {
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
                }
                mImage.Quality = quality;
                if (width > 0 && height > 0)
                    mImage.Resize(width, height);
                return mImage.ToBitmap();
            }
        }
        public static string SaveImageFile(System.IO.Stream File, string FileName, string DomainImageDirectory, int fixedHeight = 0, int fixedWidth = 0)
        {
            try
            {
                if (File == null)
                    return "";

                if (string.IsNullOrEmpty(DomainImageDirectory))
                    DomainImageDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string FileExtension = System.IO.Path.GetExtension(FileName).Replace(".", "").ToLower();
                if (FileExtension.Contains("png") || FileExtension.Contains("jpg") || FileExtension.Contains("jpeg") || FileExtension.Contains("gif") || FileExtension.Contains("bmp"))
                {
                    dynamic ID = DateTime.Now.Year + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day + "_" + DateTime.Now.Hour + "_" + DateTime.Now.Minute + "_" + DateTime.Now.Second + "_" + DateTime.Now.Millisecond;
                    if (!Directory.Exists(DomainImageDirectory + "Large/"))
                        Directory.CreateDirectory(DomainImageDirectory + "Large");
                    if (!Directory.Exists(DomainImageDirectory + "Medium/"))
                        Directory.CreateDirectory(DomainImageDirectory + "Medium");
                    if (!Directory.Exists(DomainImageDirectory + "Small/"))
                        Directory.CreateDirectory(DomainImageDirectory + "Small/");

                    var Image = System.Drawing.Image.FromStream(File);

                    Rotate(Image);

                    Image.Save(DomainImageDirectory + "Large/" + ID + "." + FileExtension);
                    int ImageWidth = Image.Width;
                    int ImageHeight = Image.Height;
                    bool ResizeBeForeCrop = false;
                    if (Image.Width > Image.Height)
                    {
                        if (Image.Width > 5000)
                        {
                            ImageHeight = 5000 * ImageHeight / ImageWidth;
                            ImageWidth = 5000;
                            ResizeBeForeCrop = true;
                        }
                    }
                    else
                    {
                        if (Image.Height > 5000)
                        {
                            ImageWidth = 5000 * ImageWidth / ImageHeight;
                            ImageHeight = 5000;
                            ResizeBeForeCrop = true;
                        }
                    }
                    if (ResizeBeForeCrop)
                    {
                        Image = new Bitmap(Image, new Size(ImageWidth, ImageHeight));
                    }

                    CropImage(Image, fixedWidth > 0 ? fixedWidth : 160, fixedHeight > 0 ? fixedHeight : 160).Save(DomainImageDirectory + "Small/" + ID + "." + FileExtension);
                    CropImage(Image, fixedWidth > 0 ? fixedWidth : 320, fixedHeight > 0 ? fixedHeight : 320).Save(DomainImageDirectory + "Medium/" + ID + "." + FileExtension);
                    CropImage(Image, fixedWidth > 0 ? fixedWidth : 640, fixedHeight > 0 ? fixedHeight : 640).Save(DomainImageDirectory + "Large/" + ID + "." + FileExtension);
                    return ID + "." + FileExtension.ToLower().Replace("ı", "i");
                }

            }
            catch (Exception)
            {
                return "";
            }
            return "";
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
