using System;

namespace Ophelia.Data.Attributes
{
    public class AllowHtml : Attribute
    {
        public bool Sanitize { get; set; }
        public bool Forbidden { get; set; }
        public AllowHtml()
        {
            this.Sanitize = true;
            this.Forbidden = false;
        }
        public AllowHtml(bool sanitize, bool forbidden)
        {
            this.Sanitize = sanitize;
            this.Forbidden = forbidden;
        }
    }
}
