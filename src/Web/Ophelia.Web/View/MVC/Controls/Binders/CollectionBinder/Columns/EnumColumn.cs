using Ophelia.Web.UI.Controls;
using Ophelia.Web.View.Mvc.Models;
using System;

namespace Ophelia.Web.View.Mvc.Controls.Binders.CollectionBinder.Columns
{
    public class EnumColumn<TModel, T> : BaseColumn<TModel, T> where T : class where TModel : ListModel<T>
    {
        public bool IsMultiple { get; set; }
        public Type EnumType { get; set; }
        public override object GetValue(T item)
        {
            var value = base.GetValue(item);
            if (!this.AllowEdit)
                return this.EnumType.GetEnumDisplayName(value, this.Binder.Client);
            else
                return value;
        }
        public override WebControl GetEditableControl(T entity, object value)
        {
            var identifierValue = this.IdentifierExpression.GetValue(entity);

            var control = new Web.UI.Controls.Select();
            control.ID = this.IdentifierKeyword + identifierValue;
            control.Name = control.ID;
            control.AddAttribute("data-identifier", Convert.ToString(identifierValue));
            control.AddAttribute("data-column", this.FormatColumnName());
            control.IsMultiple = this.IsMultiple;

            if (!this.IsMultiple)
            {
                control.CssClass = "filterbox single-value select-remote-data select2-hidden-accessible";
                control.DefaultText = this.Binder.Client.TranslateText("Select");
                control.DefaultValue = "-1";
            }
            else
            {
                control.CssClass = "filterbox multiple-value select2-hidden-accessible";
                control.AddAttribute("multiple", "multiple");
            }

            control.DataSource = this.EnumType.GetEnumSelectList(this.Binder.Client);
            if (value != null) control.SelectedValue = value.ToString();

            control.DisplayMemberName = "Text";
            control.ValueMemberName = "Value";
            control.AddAttribute("data-clear", "true");
            return control;
        }
        public EnumColumn(CollectionBinder<TModel, T> binder, string Name) : base(binder, Name)
        {

        }
    }
}
