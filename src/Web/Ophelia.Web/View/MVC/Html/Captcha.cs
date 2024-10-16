using Microsoft.AspNetCore.Http;
using Ophelia.Web.Application.Client;
using Ophelia.Web.View.Mvc.Models;
using SixLaborsCaptcha.Core;
using System;
using System.IO;

namespace Ophelia.Web.View.Mvc.Html
{
    public class Captcha : IDisposable
    {
        public string CaptchaSessionKey { get; private set; }
        private string GeneratedCode { get; set; }
        public string EncKey { get; set; }
        public CaptchaModel CaptchaModel{get; set; }

        public Captcha(CaptchaModel Model)
        {
            this.CaptchaModel = Model;
        }
        public Captcha()
        {
            this.CaptchaModel = new CaptchaModel();
        }

        private string GetRandomCode(int Length, bool UseNumerics, bool UseLowerChars, bool UseUpperChars)
        {
            Random Rand = new Random(Ophelia.Utility.Now.Millisecond);
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

        public string GetCaptchaImageString(SixLaborsCaptchaOptions options = null)
        {
            this.CaptchaSessionKey = this.GetRandomCode(10, true, false, false);
            this.GeneratedCode = this.GetRandomCode(this.CaptchaModel.CaptchaValueLength, this.CaptchaModel.UseNumerics, this.CaptchaModel.UseLowerChars, this.CaptchaModel.UseUpperChars);

            if (options == null)
            {
                options = new SixLaborsCaptchaOptions
                {
                    DrawLines = 7,
                    TextColor = new SixLabors.ImageSharp.Color[] { SixLabors.ImageSharp.Color.Blue, SixLabors.ImageSharp.Color.Black }
                };
            }
            if (string.IsNullOrEmpty(this.GeneratedCode))
                return "";

            if (Client.Current.Session != null)
                Client.Current.Session.SetString("Captcha_" + this.CaptchaSessionKey, this.GeneratedCode);
            else
                CookieManager.Set("ckey", Cryptography.CryptoManager.Encrypt($"{this.GeneratedCode}___{this.CaptchaSessionKey}", $"{this.EncKey}___{this.CaptchaSessionKey}"), new CookieOptions() { MaxAge = TimeSpan.FromMinutes(20), HttpOnly = true, Secure = Client.Current.Request.IsHttps, IsEssential = true });

            var slc = new SixLaborsCaptchaModule(options);
            var result = slc.Generate(this.GeneratedCode);
            var CaptchaImageString = "data:image/png;base64," + Convert.ToBase64String(result);
            return CaptchaImageString;
        }

        public bool CheckCaptcha(string captchaCode, string captchaSessionKey)
        {
            string captchaValue;
            if (Client.Current.Session != null)
                captchaValue = Client.Current.Session.GetString("Captcha_" + captchaSessionKey);
            else
                captchaValue = Cryptography.CryptoManager.Decrypt(CookieManager.Get("ckey"), $"{this.EncKey}___{captchaSessionKey}").Replace($"___{captchaSessionKey}", "");

            if (!string.IsNullOrEmpty(captchaCode) && !string.IsNullOrEmpty(captchaValue))
            {
                if (captchaValue.Trim().Equals(captchaCode))
                    return true;
            }
            if (Client.Current.Session != null)
            {
                int ErrorCount = Client.Current.Session.GetString("WrongEntryCount") != null ? Int32.Parse(Client.Current.Session.GetString("WrongEntryCount")) : 0;
                Client.Current.Session.SetString("WrongEntryCount", (ErrorCount + 1).ToString());
            }
            return false;
        }

        private bool IsWrongEntryCountExceed()
        {
            if (Client.Current.Session != null)
            {
                if (Client.Current.Session.GetString("WrongEntryCount") == null)
                    Client.Current.Session.SetString("WrongEntryCount", "0");
                else if (Client.Current.Session.GetString("WrongEntryCount").ToInt32() >= this.CaptchaModel.ErrorCount)
                    return true;
            }
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
            if (Client.Current.Session != null && Client.Current.Session.GetString("Captcha_" + KeyName) != null)
                Client.Current.Session.Remove("Captcha_" + KeyName);
            else
                CookieManager.ClearByName("ckey");
        }
        public void IncreaseCaptchaErrorCount()
        {
            if (Client.Current.Session != null)
            {
                int ErrorCount = Client.Current.Session.GetString("WrongEntryCount") != null ? Int32.Parse(Client.Current.Session.GetString("WrongEntryCount")) : 0;
                Client.Current.Session.SetString("WrongEntryCount", (ErrorCount + 1).ToString());
            }
        }
        public void ClearErrorCount()
        {
            if (Client.Current.Session != null && Client.Current.Session.GetString("WrongEntryCount") != null)
                Client.Current.Session.Remove("WrongEntryCount");
        }
    }
}
