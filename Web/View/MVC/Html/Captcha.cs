using Microsoft.AspNetCore.Http;
using Ophelia.Web.View.Mvc.Models;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace Ophelia.Web.View.Mvc.Html
{
    public class Captcha : IDisposable
    {
        private CaptchaModel mCaptchaModel;
        private string sCaptchaSessionKey;
        public string CaptchaSessionKey { get { return this.sCaptchaSessionKey; } }

        public CaptchaModel CaptchaModel
        {
            get
            {
                return this.mCaptchaModel;
            }
            set
            {
                this.mCaptchaModel = value;
            }
        }
        public Captcha(CaptchaModel Model)
        {
            this.CaptchaModel = Model;
        }
        public Captcha()
        {

            this.CaptchaModel = new CaptchaModel();
        }
        private void GenerateCaptchaValue()
        {
            this.sCaptchaSessionKey = this.GetRandomCode(10, true, false, false);
            Client.Current.Session.SetString("Captcha_" + this.CaptchaSessionKey, this.GetRandomCode(this.CaptchaModel.CaptchaValueLength, this.CaptchaModel.UseNumerics, this.CaptchaModel.UseLowerChars, this.CaptchaModel.UseUpperChars));
        }

        private string GetRandomCode(int Length, bool UseNumerics, bool UseLowerChars, bool UseUpperChars)
        {
            Random Rand = new Random(DateTime.Now.Millisecond);
            char[] CharacterSet = new char[62];
            string RandomValue = string.Empty;
            int CopiedCharacterLength = 0;
            if (UseNumerics)
            {
                "123456789".ToCharArray().CopyTo(CharacterSet, 0);
                CopiedCharacterLength = 9;
            }
            if (UseLowerChars)
            {
                "abcdefghjkmnpqrstuvwxyz".ToCharArray().CopyTo(CharacterSet, CopiedCharacterLength);
                CopiedCharacterLength += 23;
            }
            if (UseUpperChars)
            {
                "ABCDEFGHJKLMNPQRSTUVWXYZ".ToCharArray().CopyTo(CharacterSet, CopiedCharacterLength);
                CopiedCharacterLength += 24;
            }

            for (int i = 1; i <= Length; i++)
                RandomValue += CharacterSet[Rand.Next(0, CopiedCharacterLength - 1)].ToString();
            return RandomValue;
        }

        public string GetCaptchaImageString()
        {
            this.GenerateCaptchaValue();
            Bitmap CaptchaImage = new Bitmap(this.CaptchaModel.Width, this.CaptchaModel.Height);
            Graphics Graphic = Graphics.FromImage(CaptchaImage);
            Graphic.SmoothingMode = SmoothingMode.HighQuality;
            Rectangle CaptchaContainer = new Rectangle(0, 0, this.CaptchaModel.Width, this.CaptchaModel.Height);
            HatchBrush CaptchaBackground = new HatchBrush(HatchStyle.SmallConfetti, Color.LightSlateGray, Color.White);
            Graphic.FillRectangle(CaptchaBackground, CaptchaContainer);

            Font Font = new Font(this.CaptchaModel.FontFamily, this.CaptchaModel.FontSize, FontStyle.Bold);
            SizeF Size = Graphic.MeasureString(Client.Current.Session.GetString("Captcha_" + this.CaptchaSessionKey), Font);
            StringFormat Format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            GraphicsPath Path = new GraphicsPath();
            Path.AddString(Client.Current.Session.GetString("Captcha_" + this.CaptchaSessionKey), Font.FontFamily, (int)Font.Style, this.CaptchaModel.FontSize, CaptchaContainer, Format);

            Font.Dispose();
            Font = null;

            float v = 3.5F;
            Random Rand = new Random(DateTime.Now.Millisecond);
            int RandomWidth = Rand.Next(CaptchaContainer.Width);
            int RandomHeight = Rand.Next(CaptchaContainer.Height);
            PointF[] Points =
            {
                new PointF(RandomWidth / v, RandomHeight / v),
                new PointF(CaptchaContainer.Width - RandomWidth / v, RandomHeight / v),
                new PointF(RandomWidth / v, CaptchaContainer.Height - RandomHeight / v),
                new PointF(CaptchaContainer.Width - RandomWidth / v, CaptchaContainer.Height - RandomHeight / v)
            };

            Matrix Matrix = new Matrix();
            Matrix.Translate(0F, 0F);
            Path.Warp(Points, CaptchaContainer, Matrix, WarpMode.Perspective, 0F);

            CaptchaBackground = new HatchBrush(HatchStyle.LargeConfetti, Color.FromArgb(130, 130, 130), Color.FromArgb(130, 130, 130));
            Graphic.FillPath(CaptchaBackground, Path);

            int m = Math.Max(CaptchaContainer.Width, CaptchaContainer.Height);
            for (int i = 0; i < (int)(CaptchaContainer.Width * CaptchaContainer.Height / 30F); i++)
            {
                int x = Rand.Next(CaptchaContainer.Width);
                int y = Rand.Next(CaptchaContainer.Height);
                int w = Rand.Next(m / 50);
                int h = Rand.Next(m / 50);
                Graphic.FillEllipse(CaptchaBackground, x, y, w, h);
            }
            CaptchaBackground.Dispose();
            CaptchaBackground = null;
            Graphic.Dispose();
            Graphic = null;

            string CaptchaImageString = string.Empty;
            using (MemoryStream MemoryStream = new MemoryStream())
            {
                CaptchaImage.Save(MemoryStream, System.Drawing.Imaging.ImageFormat.Png);
                CaptchaImageString = "data:image/png;base64," + Convert.ToBase64String(MemoryStream.ToArray());
            }
            return CaptchaImageString;
        }

        public bool CheckCaptcha(string CaptchaCode, string CaptchaSessionKey)
        {
            if (!string.IsNullOrEmpty(CaptchaCode) && Client.Current.Session.GetString("Captcha_" + CaptchaSessionKey) != null)
            {
                if (Client.Current.Session.GetString("Captcha_" + CaptchaSessionKey).Trim().Equals(CaptchaCode))
                    return true;
            }
            int ErrorCount = Client.Current.Session.GetString("WrongEntryCount") != null ? Int32.Parse(Client.Current.Session.GetString("WrongEntryCount")) : 0;
            Client.Current.Session.SetString("WrongEntryCount", (ErrorCount + 1).ToString());
            return false;
        }

        private bool IsWrongEntryCountExceed()
        {
            if (Client.Current.Session.GetString("WrongEntryCount") == null)
                Client.Current.Session.SetString("WrongEntryCount", "0");
            else if (Int32.Parse(Client.Current.Session.GetString("WrongEntryCount")) >= this.CaptchaModel.ErrorCount)
                return true;
            return false;
        }

        public bool Enabled()
        {
            return this.CaptchaModel.AlwaysShow || this.IsWrongEntryCountExceed();
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public void DeleteSessionKey(string KeyName)
        {
            if (Client.Current.Session.GetString("Captcha_" + KeyName) != null)
                Client.Current.Session.Remove("Captcha_" + KeyName);
        }
        public void IncreaseCaptchaErrorCount()
        {
            int ErrorCount = Client.Current.Session.GetString("WrongEntryCount") != null ? Int32.Parse(Client.Current.Session.GetString("WrongEntryCount")) : 0;
            Client.Current.Session.SetString("WrongEntryCount", (ErrorCount + 1).ToString());
        }
        public void ClearErrorCount()
        {
            if (Client.Current.Session.GetString("WrongEntryCount") != null)
                Client.Current.Session.Remove("WrongEntryCount");
        }
    }
}
