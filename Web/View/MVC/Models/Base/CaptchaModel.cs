using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Web.View.Mvc.Models
{
    public class CaptchaModel : BaseModel
    {
        private int nCaptchaValueLength, nWidth, nHeight;
        private string sFontFamily;

        public CaptchaModel()
        {
            this.nWidth = 100;
            this.nHeight = 30;
            this.nCaptchaValueLength = 5;
            this.FontFamily = "Helvetica";
            this.UseNumerics = true;
            this.UseLowerChars = false;
            this.UseUpperChars = false;
            this.ErrorCount = 0;
            this.FontSize = 20;
            this.AlwaysShow = false;
            base.AlertMessage = new AlertMessageModel { Message = "Lütfen güvenlik kodunu doğru şekilde giriniz." };
        }
        public bool UseNumerics { get; set; }
        public bool UseLowerChars { get; set; }
        public bool UseUpperChars { get; set; }
        public int ErrorCount { get; set; }
        public bool AlwaysShow { get; set; }
        public int FontSize { get; set; }
        public int Width
        {
            get
            {
                return this.nWidth;
            }
            set
            {
                if (value < this.CaptchaValueLength * 15)
                    this.nWidth = this.CaptchaValueLength * 15;
                else
                    this.nWidth = value;
            }
        }
        public int Height
        {
            get
            {
                return this.nHeight;
            }
            set
            {
                if (value < 30)
                    this.nHeight = 30;
                else
                    this.nHeight = value;
            }
        }
        public int CaptchaValueLength
        {
            get
            {
                return this.nCaptchaValueLength;
            }
            set
            {
                if (value < 5)
                    this.nCaptchaValueLength = 5;
                else
                    this.nCaptchaValueLength = value;
            }
        }
        public string FontFamily
        {
            get
            {
                if (string.IsNullOrEmpty(this.sFontFamily))
                    this.sFontFamily = "Helvetica";
                return this.sFontFamily;
            }
            set
            {
                this.sFontFamily = value;
            }
        }
    }
}
