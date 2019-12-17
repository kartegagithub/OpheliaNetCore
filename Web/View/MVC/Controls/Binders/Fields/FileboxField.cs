using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ophelia.Web.UI.Controls;
using Ophelia;


namespace Ophelia.Web.View.Mvc.Controls.Binders.Fields
{
    public class FileboxField<T> : BaseField<T> where T : class
    {
        public new Textbox DataControl { get { return (Textbox)base.DataControl; } set { base.DataControl = value; } }

        protected override WebControl CreateDataControl()
        {
            return new Textbox();
        }
        protected override void onBeforeRenderControl(TextWriter writer)
        {
            base.onBeforeRenderControl(writer);
            this.DataControl.CssClass = "form-control file-styled-primary";
            this.DataControl.AddAttribute("data-text", this.Client.TranslateText("Select"));
            this.DataControl.AddAttribute("data-placeholder", this.Client.TranslateText("NoFileSelected"));
            if (this.ExpressionValue != null && !string.IsNullOrEmpty(Convert.ToString(this.ExpressionValue)))
            {
                this.DataControl.AddAttribute("data-file-name", System.IO.Path.GetFileName(Convert.ToString(this.ExpressionValue)));
                this.DataControl.AddAttribute("data-file-path", this.Client.GetImagePath(Convert.ToString(this.ExpressionValue)));
            }
        }
        public FileboxField(FieldContainer<T> FieldContainer) :base(FieldContainer)
        {
            this.DataControl.Type = "file";
        }
    }
}
