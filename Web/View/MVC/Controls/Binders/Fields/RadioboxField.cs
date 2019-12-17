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
    public class RadioboxField<T> : BaseField<T> where T : class
    {
        public new Radio DataControl { get { return (Radio)base.DataControl; } set { base.DataControl = value; } }

        protected override WebControl CreateDataControl()
        {
            return new Radio();
        }
        protected override void onBeforeRenderControl(TextWriter writer)
        {
            base.onBeforeRenderControl(writer);
            if (this.ExpressionValue != null)
                this.DataControl.Checked = Convert.ToBoolean(this.ExpressionValue);
        }
        public RadioboxField(FieldContainer<T> FieldContainer) :base(FieldContainer)
        {
            
        }
    }
}
