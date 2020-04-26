using System.IO;

namespace Ophelia.Web.UI.Controls
{
    public class Link : WebControl
    {
        public string URL { get; set; }
        public string OnClick { get; set; }
        public bool NewWindow { get; set; }
        public string Text { get; set; }
        public string Title { get; set; }
        protected override void onBeforeRenderControl(TextWriter writer)
        {
            if (!string.IsNullOrEmpty(this.URL))
                this.AddAttribute("href", this.URL);
            else
                this.AddAttribute("href", "#");
            if (!string.IsNullOrEmpty(this.OnClick))
            {
                this.AddAttribute("onclick", this.OnClick);
            }
            if (!string.IsNullOrEmpty(this.Text) && string.IsNullOrEmpty(this.Title) && this.Text.IndexOf("<") == -1)
            {
                this.Title = this.Text;
            }
            if (!string.IsNullOrEmpty(this.Title))
            {
                this.AddAttribute("title", this.Title);
            }
            if (this.NewWindow)
            {
                this.AddAttribute("target", "_blank");
            }
            base.onBeforeRenderControl(writer);
        }

        protected override void RenderContents(TextWriter writer)
        {
            writer.Write(this.Text);
            base.RenderContents(writer);
        }
        public Link() : base(TextWriterTag.A)
        {

        }
    }
}
