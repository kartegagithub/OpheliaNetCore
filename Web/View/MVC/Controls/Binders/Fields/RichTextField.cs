using Ophelia.Web.UI.Controls;
using System;
using System.IO;

namespace Ophelia.Web.View.Mvc.Controls.Binders.Fields
{
    public class RichTextField<T> : BaseField<T> where T : class
    {
        public new TextArea DataControl { get { return (TextArea)base.DataControl; } set { base.DataControl = value; } }

        protected override WebControl CreateDataControl()
        {
            return new TextArea();
        }
        protected override void onBeforeRenderControl(TextWriter writer)
        {
            base.onBeforeRenderControl(writer);
            this.DataControl.CssClass += " richtext";
            if (this.ExpressionValue != null)
                this.DataControl.Value = Convert.ToString(this.ExpressionValue);
        }
        public RichTextField(FieldContainer<T> FieldContainer) : base(FieldContainer)
        {

        }
    }
}
