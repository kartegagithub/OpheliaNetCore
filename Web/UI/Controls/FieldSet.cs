using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Web.UI.Controls
{
    public class FieldSet : WebControl
    {
        public Legend Legend { get; set; }

        public FieldSet() : base(TextWriterTag.Fieldset)
        {
            this.Legend = new Legend();
            this.Controls.Add(this.Legend);
        }
    }

    public class Legend : WebControl
    {
        public string Text { get; set; }

        public override void RenderControl(TextWriter writer)
        {
            if (!string.IsNullOrEmpty(this.Text))
                writer.Write(this.Text);
            base.RenderControl(writer);
        }
        public Legend() : base(TextWriterTag.Legend)
        {

        }
    }
}
