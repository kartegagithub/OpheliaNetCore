using Microsoft.AspNetCore.Http;
using Ophelia.Web.View.Mvc.Models;
using SixLaborsCaptcha.Core;
using System;
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
            var slc = new SixLaborsCaptchaModule(new SixLaborsCaptchaOptions
            {
                DrawLines = 7,
                TextColor = new SixLabors.ImageSharp.Color[] { SixLabors.ImageSharp.Color.Blue, SixLabors.ImageSharp.Color.Black },
            });
            var captchaKey = Client.Current.Session.GetString("Captcha_" + this.CaptchaSessionKey);
            var result = slc.Generate(captchaKey);
            var CaptchaImageString = "data:image/png;base64," + Convert.ToBase64String(result);
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
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {

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
