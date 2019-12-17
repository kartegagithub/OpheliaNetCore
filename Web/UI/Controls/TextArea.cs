using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Web.UI.Controls
{
    public class TextArea : WebControl
    {
        public string Value { get; set; }

        protected override void RenderContents(TextWriter writer)
        {
            writer.Write(this.Value);
            base.RenderContents(writer);
        }

        public TextArea() : base(TextWriterTag.Textarea)
        {
            this.CssClass = "form-control";
        }
    }
}
