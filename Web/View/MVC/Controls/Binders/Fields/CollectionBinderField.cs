using Microsoft.AspNetCore.Html;
using Ophelia.Web.UI.Controls;
using Ophelia.Web.View.Mvc.Models;
using System.IO;

namespace Ophelia.Web.View.Mvc.Controls.Binders.Fields
{
    public class CollectionBinderField<TModel, TEntity, T> : BaseField<T> where T : class where TEntity : class where TModel : ListModel<TEntity>
    {
        public new CollectionBinder.CollectionBinder<TModel, TEntity> DataControl { get { return (CollectionBinder.CollectionBinder<TModel, TEntity>)base.DataControl; } set { base.DataControl = value; } }

        protected override WebControl CreateDataControl()
        {
            return null;
        }
        public override HtmlString Render()
        {
            if (this.Visible)
            {
                this.onBeforeRenderControl(this.Output);
                this.DataControl.RenderContent();
            }
            return new HtmlString("");
        }
        protected override void onBeforeRenderControl(TextWriter writer)
        {
            base.onBeforeRenderControl(writer);
        }
        public CollectionBinderField(FieldContainer<T> FieldContainer, CollectionBinder.CollectionBinder<TModel, TEntity> binder, bool hideLabel = false) : base(FieldContainer)
        {
            this.DataControl = binder;
            this.DataControlParent.Controls.Add(this.DataControl);
            this.ID = this.DataControl.ID;
            this.Text = this.DataControl.Title;
            this.LabelControl.Visible = !hideLabel;
            this.CanTranslateLabelText = false;
        }
    }
}
