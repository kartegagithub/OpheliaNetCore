using System.IO;

namespace Ophelia.Web.UI.Controls
{
    public class Image : WebControl
    {
        public string Source { get; set; }

        protected override void onBeforeRenderControl(TextWriter writer)
        {
            this.AddAttribute("src", this.Source);
            base.onBeforeRenderControl(writer);
        }
        public Image() : base(TextWriterTag.Img)
        {

        }
    }
}
