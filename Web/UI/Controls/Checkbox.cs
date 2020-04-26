using System.IO;

namespace Ophelia.Web.UI.Controls
{
    public class Checkbox : WebControl
    {
        public bool Checked { get; set; }
        public string Value { get; set; }
        public string DataOnText { get; set; }
        public string DataOffText { get; set; }

        protected override void onBeforeRenderControl(TextWriter writer)
        {
            if (this.Checked)
                this.AddAttribute("checked", "checked");
            if (!string.IsNullOrEmpty(this.Value))
                this.AddAttribute("value", this.Value);
            this.AddAttribute("type", "checkbox");
            base.onBeforeRenderControl(writer);
        }

        public Checkbox() : base(TextWriterTag.Input)
        {

        }
    }
}
