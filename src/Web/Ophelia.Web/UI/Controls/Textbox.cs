using System.IO;

namespace Ophelia.Web.UI.Controls
{
    public class Textbox : WebControl
    {
        public string Type { get; set; }
        public string Value { get; set; }

        protected override void onBeforeRenderControl(TextWriter writer)
        {
            this.AddAttribute("value", this.Value);
            this.AddAttribute("type", this.Type);
            this.AddAttribute("autocomplete", "off");
            base.onBeforeRenderControl(writer);
        }

        public Textbox() : base(TextWriterTag.Input)
        {
            this.Type = "text";
            this.CssClass = "form-control";
        }
    }
}
