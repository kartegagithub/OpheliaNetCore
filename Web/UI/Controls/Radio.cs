using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Ophelia.Web.UI.Controls
{
    public class Radio : WebControl
    {
        public bool Checked { get; set; }
        public string Value { get; set; }

        protected override void onBeforeRenderControl(TextWriter writer)
        {
            if(this.Checked)
                this.AddAttribute("checked", "checked");
            if(!string.IsNullOrEmpty(this.Value))
                this.AddAttribute("value", this.Value);
            this.AddAttribute("type", "radio");
            base.onBeforeRenderControl(writer);
        }

        public Radio() : base(TextWriterTag.Input)
        {

        }
    }
}
