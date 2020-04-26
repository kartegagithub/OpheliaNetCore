using System.IO;

namespace Ophelia.Web.UI.Controls
{
    public class Label : WebControl
    {
        public string Text { get; set; }

        protected override void RenderContents(TextWriter writer)
        {
            writer.Write(this.Text);
            base.RenderContents(writer);
        }
        public Label() : base(TextWriterTag.Label)
        {

        }
    }
}
