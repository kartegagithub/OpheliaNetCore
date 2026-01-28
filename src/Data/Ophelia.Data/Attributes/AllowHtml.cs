using System;

namespace Ophelia.Data.Attributes
{
    public class AllowHtml : Attribute
    {
        public string[] AllowedTags { get; set; }
        public string[] AllowedSchemes { get; set; }
        public string[] AllowedAttributes { get; set; }
        public bool Sanitize { get; set; }
        public bool Forbidden { get; set; }
        public AllowHtml()
        {
            this.Sanitize = true;
            this.Forbidden = false;
        }
        public AllowHtml(bool sanitize, bool forbidden, string[] allowedTags, string[] allowedSchemes, string[] allowedAttributes)
        {
            this.Sanitize = sanitize;
            this.Forbidden = forbidden;
            this.AllowedTags = allowedTags;
            this.AllowedSchemes = allowedSchemes;
            this.AllowedAttributes = allowedAttributes;
        }
    }
}
