using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Ophelia.Drawing
{
    public static class ImageResizer
    {
        public static System.Drawing.Image Rotate(System.Drawing.Image originalImage)
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

            }
            return originalImage;
        }

        public static System.Drawing.Bitmap CropImage(byte[] data, int Width, int Height)
        {
            return CropImage(System.Drawing.Bitmap.FromStream(new System.IO.MemoryStream(data)), Width, Height);
        }

        public static System.Drawing.Bitmap CropImage(System.Drawing.Image Image, int Width, int Height)
        {
            if ((Image != null))
            {
                Rotate(Image);
                if (Image.Width > Width || Image.Height > Height)
                {
                    decimal Ratio = Convert.ToDecimal(Image.Width) / Convert.ToDecimal(Image.Height);
                    int SizedWidth = Image.Width;
                    int SizedHeight = Image.Height;
                    if (Ratio > 1)
                    {
                        if (Height > Image.Height)
                            Height = Image.Height;
                        SizedHeight = Height;
                        SizedWidth = Convert.ToInt32(SizedHeight * Ratio);
                    }
                    else
                    {
                        if (Width > Image.Width)
                            Width = Image.Width;
                        SizedWidth = Width;
                        SizedHeight = Convert.ToInt32(SizedWidth / Ratio);
                    }

                    System.Drawing.Bitmap NewImage = null;
                    if (Image.PixelFormat.ToString().Contains("Indexed"))
                    {
                        NewImage = new System.Drawing.Bitmap(Convert.ToInt32(SizedWidth), Convert.ToInt32(SizedHeight));
                    }
                    else
                    {
                        NewImage = new System.Drawing.Bitmap(SizedWidth, SizedHeight, Image.PixelFormat);
                    }
                    System.Drawing.Graphics Graph = System.Drawing.Graphics.FromImage(NewImage);
                    Graph.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    Graph.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    Graph.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    Graph.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    System.Drawing.Rectangle Rect = new System.Drawing.Rectangle(0, 0, SizedWidth, SizedHeight);
                    Graph.DrawImage(Image, Rect);

                    System.Drawing.Rectangle CroppedRect = new System.Drawing.Rectangle((NewImage.Width - Width) / 2, (NewImage.Height - Height) / 2, Width, Height);
                    System.Drawing.Bitmap BMP = NewImage.Clone(CroppedRect, NewImage.PixelFormat);
                    return BMP;
                }
                else
                {
                    System.Drawing.Bitmap NewImage = null;
                    if (Image.PixelFormat.ToString().Contains("Indexed"))
                    {
                        NewImage = new System.Drawing.Bitmap(Convert.ToInt32(Image.Width), Convert.ToInt32(Image.Height));
                    }
                    else
                    {
                        NewImage = new System.Drawing.Bitmap(Image.Width, Image.Height, Image.PixelFormat);
                    }
                    System.Drawing.Graphics Graph = System.Drawing.Graphics.FromImage(NewImage);
                    Graph.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    Graph.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    Graph.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    Graph.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    System.Drawing.Rectangle Rect = new System.Drawing.Rectangle(0, 0, Image.Width, Image.Height);
                    Graph.DrawImage(Image, Rect);
                    return NewImage;
                }
            }
            return null;
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
                    if (!System.IO.Directory.Exists(DomainImageDirectory + "Large/"))
                        System.IO.Directory.CreateDirectory(DomainImageDirectory + "Large");
                    if (!System.IO.Directory.Exists(DomainImageDirectory + "Medium/"))
                        System.IO.Directory.CreateDirectory(DomainImageDirectory + "Medium");
                    if (!System.IO.Directory.Exists(DomainImageDirectory + "Small/"))
                        System.IO.Directory.CreateDirectory(DomainImageDirectory + "Small/");

                    System.Drawing.Image Image = System.Drawing.Image.FromStream(File);

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
                        Image = new System.Drawing.Bitmap(Image, new System.Drawing.Size(ImageWidth, ImageHeight));
                    }

                    CropImage(Image, fixedWidth > 0 ? fixedWidth : 160, fixedHeight > 0 ? fixedHeight : 160).Save(DomainImageDirectory + "Small/" + ID + "." + FileExtension);
                    CropImage(Image, fixedWidth > 0 ? fixedWidth : 320, fixedHeight > 0 ? fixedHeight : 320).Save(DomainImageDirectory + "Medium/" + ID + "." + FileExtension);
                    CropImage(Image, fixedWidth > 0 ? fixedWidth : 640, fixedHeight > 0 ? fixedHeight : 640).Save(DomainImageDirectory + "Large/" + ID + "." + FileExtension);
                    return ID + "." + FileExtension.ToLower().Replace("ı", "i");
                }

            }
            catch (Exception)
            {
            }
            return "";
        }
        public static System.Drawing.Image ResizeImage(System.Drawing.Image BMP, int Width, int Height, string PathToSave)
        {
            try
            {
                Rotate(BMP);

                if (BMP.Width > Width)
                {
                    Height = Width * BMP.Height / BMP.Width;
                }
                else if (BMP.Height > Height)
                {
                    Width = Height * BMP.Width / BMP.Height;
                }
                else
                {
                    Width = BMP.Width;
                    Height = BMP.Height;
                }
                System.Drawing.Bitmap NewBMP = new System.Drawing.Bitmap(BMP, Width, Height);
                if (!string.IsNullOrEmpty(PathToSave))
                    NewBMP.Save(PathToSave);
                return NewBMP;
            }
            catch (Exception)
            {
                return null;
            }
        }
       
        public static void ResizeImage(System.IO.Stream InputStream, int Width, int Height, string PathToSave)
        {
            try
            {
                System.Drawing.Image BMP = System.Drawing.Bitmap.FromStream(InputStream);

                Rotate(BMP);

                if (BMP.Width > Width)
                {
                    Height = Width * BMP.Height / BMP.Width;
                }
                else if (BMP.Height > Height)
                {
                    Width = Height * BMP.Width / BMP.Height;
                }
                else
                {
                    Width = BMP.Width;
                    Height = BMP.Height;
                }
                System.Drawing.Bitmap NewBMP = new System.Drawing.Bitmap(BMP, Width, Height);
                NewBMP.Save(PathToSave);
            }
            catch (Exception)
            {
            }
        }
        public static System.Drawing.Bitmap ResizeImage(byte[] data, int Width, int Height)
        {
            try
            {
                System.Drawing.Image BMP = System.Drawing.Bitmap.FromStream(new System.IO.MemoryStream(data));

                Rotate(BMP);

                if (BMP.Width > Width)
                {
                    Height = Width * BMP.Height / BMP.Width;
                }
                else if (BMP.Height > Height)
                {
                    Width = Height * BMP.Width / BMP.Height;
                }
                else {
                    Width = BMP.Width;
                    Height = BMP.Height;
                }
                System.Drawing.Bitmap NewBMP = new System.Drawing.Bitmap(BMP, Width, Height);
                return NewBMP;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
