using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Web.UI.Controls
{
    public class HiddenInput : WebControl
    {
        public string Value { get; set; }

        protected override void onBeforeRenderControl(TextWriter writer)
        {
            this.AddAttribute("value", this.Value);
            this.AddAttribute("type", "hidden");
            base.onBeforeRenderControl(writer);
        }

        public HiddenInput() : base(TextWriterTag.Input)
        {
            
        }
    }
}
