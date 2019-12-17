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
    public class LabelField<T> : BaseField<T> where T : class
    {
        public Type EnumType { get; set; }
        public string Value { get; set; }
        public new Label DataControl { get { return (Label)base.DataControl; } set { base.DataControl = value; } }

        protected override WebControl CreateDataControl()
        {
            return new Label();
        }
        protected override void onBeforeRenderControl(TextWriter writer)
        {
            base.onBeforeRenderControl(writer);
            if (this.ExpressionValue != null)
            {
                if(this.EnumType == null)
                    this.DataControl.Text = Convert.ToString(this.ExpressionValue);
                else
                {
                    this.DataControl.Text = this.EnumType.GetEnumDisplayName(this.ExpressionValue, this.Client);
                }
            }
            else if(!string.IsNullOrEmpty(this.Value))
            {
                this.DataControl.Text = this.Value;
            }
        }
        public LabelField(FieldContainer<T> FieldContainer) :base(FieldContainer)
        {
            this.DataControlParent.CssClass += " border-bottom-ccc border-bottom";
        }
    }
}
