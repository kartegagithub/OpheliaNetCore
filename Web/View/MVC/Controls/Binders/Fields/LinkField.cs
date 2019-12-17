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
    public class LinkField<T> : BaseField<T> where T : class
    {
        public new Link DataControl { get { return (Link)base.DataControl; } set { base.DataControl = value; } }

        protected override WebControl CreateDataControl()
        {
            return new Link();
        }
        protected override void onBeforeRenderControl(TextWriter writer)
        {
            base.onBeforeRenderControl(writer);
                
        }
        public LinkField(FieldContainer<T> FieldContainer) :base(FieldContainer)
        {
            this.DataControlParent.CssClass += " border-bottom-ccc border-bottom";
        }
    }
}
