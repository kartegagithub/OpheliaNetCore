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
    public class PasswordField<T> : BaseField<T> where T : class
    {
        public new Passwordbox DataControl { get { return (Passwordbox)base.DataControl; } set { base.DataControl = value; } }

        protected override WebControl CreateDataControl()
        {
            return new Passwordbox();
        }
        protected override void onBeforeRenderControl(TextWriter writer)
        {
            base.onBeforeRenderControl(writer);
            if (this.ExpressionValue != null)
                this.DataControl.Value = Convert.ToString(this.ExpressionValue);
        }
        public PasswordField(FieldContainer<T> FieldContainer) :base(FieldContainer)
        {
            
        }
    }
}
