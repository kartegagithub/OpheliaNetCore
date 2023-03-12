using Ophelia.Web.UI.Controls;
using Ophelia.Web.View.Mvc.Models;
using System;

namespace Ophelia.Web.View.Mvc.Controls.Binders.CollectionBinder.Columns
{
    public class NumericColumn<TModel, T> : TextColumn<TModel, T> where T : class where TModel : ListModel<T>
    {
        public string Format { get; set; }
        public override object GetValue(T item)
        {
            this.SetFormat(this.Format, this.Binder.Configuration.DecimalFormat, this.Binder.Configuration.IntFormat);

            var value = base.GetValue(item);
            if (value == null)
                return 0.ToString(this.Format);
            if (string.IsNullOrEmpty(this.Format))
                return value;
            else
                return Convert.ToDecimal(value).ToString(this.Format);
        }
        public override WebControl GetEditableControl(T entity, object value)
        {
            var identifierValue = this.IdentifierExpression.GetValue(entity);

            var textbox = new Textbox();
            textbox.ID = this.IdentifierKeyword + identifierValue;
            textbox.AddAttribute("data-identifier", Convert.ToString(identifierValue));
            textbox.AddAttribute("data-column", this.FormatColumnName());
            textbox.Name = textbox.ID;
            textbox.HtmlAttributes = this.HtmlAttributes;
            textbox.CssClass = "form-control numeric";
            if (BinderConfiguration.UseHtml5DataTypes)
                textbox.Type = "number";
            textbox.Value = Convert.ToString(this.GetValue(entity));
            this.SetAttributes(textbox);
            return textbox;
        }

        public void SetFormat(string format, string defaultDecimalFormat, string defaultIntFormat)
        {
            if (format == "-")
                return;
            if (!string.IsNullOrEmpty(format))
                this.Format = format;
            else
            {
                if (!string.IsNullOrEmpty(defaultDecimalFormat) || !string.IsNullOrEmpty(defaultIntFormat))
                {
                    var propType = this.Expression.GetPropertyType();
                    if (propType == null)
                        return;

                    if (!string.IsNullOrEmpty(defaultDecimalFormat) && propType.Name.IndexOf("Decimal", StringComparison.InvariantCultureIgnoreCase) > -1)
                    {
                        this.Format = defaultDecimalFormat;
                    }
                    else if (!string.IsNullOrEmpty(defaultDecimalFormat) &&
                        (propType.Name.IndexOf("long", StringComparison.InvariantCultureIgnoreCase) > -1
                        || propType.Name.IndexOf("int", StringComparison.InvariantCultureIgnoreCase) > -1
                        || propType.Name.IndexOf("int16", StringComparison.InvariantCultureIgnoreCase) > -1
                        || propType.Name.IndexOf("int32", StringComparison.InvariantCultureIgnoreCase) > -1
                        || propType.Name.IndexOf("int64", StringComparison.InvariantCultureIgnoreCase) > -1))
                    {
                        this.Format = defaultIntFormat;
                    }
                }
            }
        }

        public NumericColumn(CollectionBinder<TModel, T> binder, string Name) : base(binder, Name)
        {
            this.Alignment = HorizontalAlign.Right;
        }
    }
}
