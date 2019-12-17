using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Web.UI.Controls
{
    public class Literal : WebControl
    {
        public string Text { get; set; }

        public override void RenderControl(TextWriter writer)
        {
            writer.Write(this.Text);
        }
        public Literal() : base(TextWriterTag.Label)
        {

        }
        public Literal(string content) : base(TextWriterTag.Label)
        {
            this.Text = content;
        }
    }
}
