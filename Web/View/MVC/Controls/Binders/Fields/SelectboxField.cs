using Ophelia.Web.UI.Controls;
using System;
using System.IO;


namespace Ophelia.Web.View.Mvc.Controls.Binders.Fields
{
    public class SelectboxField<T> : BaseField<T> where T : class
    {
        public string NewURL { get; set; }
        public string ViewURL { get; set; }
        public bool AllowNew { get; set; }
        public bool AllowView { get; set; }
        public new Select DataControl { get { return (Select)base.DataControl; } set { base.DataControl = value; } }

        protected override WebControl CreateDataControl()
        {
            return new Select();
        }
        protected override void onBeforeRenderControl(TextWriter writer)
        {
            base.onBeforeRenderControl(writer);
            if (this.DefaultValue == null)
                this.DefaultValue = "-1";

            if (!this.DataControl.IsMultiple)
                this.DataControl.CssClass += " form-control simple-select select-menu-color select2-hidden-accessible";
            else
            {
                this.DataControl.CssClass += " form-control simple-select multiple-selection select2-hidden-accessible";
                this.DataControl.AddAttribute("multiple", "multiple");
            }

            if (this.IsRequired && !string.IsNullOrEmpty(this.DataControl.CssClass) && this.DataControl.CssClass.IndexOf(" required") == -1)
            {
                this.DataControl.CssClass += " required";
                this.DataControl.AddAttribute("aria-required", "true");
            }

            if (this.ExpressionValue != null)
                this.DataControl.SelectedValue = Convert.ToString(this.ExpressionValue);

            if (this.AllowNew)
            {
                this.DataControl.AddAttribute("data-allow-new", "true");
                this.DataControl.AddAttribute("data-new-url", this.NewURL);
            }
            if (this.AllowView)
            {
                this.DataControl.AddAttribute("data-allow-view", "true");
                this.DataControl.AddAttribute("data-view-url", this.ViewURL);
            }
            this.DataControl.AddAttribute("data-clear", "true");
            this.HasValue = !string.IsNullOrEmpty(this.DataControl.SelectedValue) && this.DataControl.SelectedValue != Convert.ToString(this.DefaultValue);
        }
        public SelectboxField(FieldContainer<T> FieldContainer) : base(FieldContainer)
        {

        }
    }
}
