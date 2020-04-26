using Ophelia.Web.UI.Controls;
using System;
using System.IO;
using System.Linq.Expressions;

namespace Ophelia.Web.View.Mvc.Controls.Binders.Fields
{
    public class MoneyField<T> : TextboxField<T> where T : class
    {
        public new Textbox DataControl { get { return base.DataControl; } set { base.DataControl = value; } }
        public Select CurrencyControl { get; private set; }
        public Expression<Func<T, object>> CurrencyExpression { get; set; }
        public string Format { get; set; }
        public bool CurrencyReadOnly { get; set; }

        protected override WebControl CreateDataControl()
        {
            return new Textbox();
        }
        protected override void onBeforeRenderControl(TextWriter writer)
        {
            base.onBeforeRenderControl(writer);
            this.DataControl.CssClass += " numeric";
            this.DataControl.Value = this.FormatValue(this.DataControl.Value);
            var currency = this.CurrencyExpression.GetValue(this.FieldContainer.Entity);
            if (!this.CurrencyReadOnly)
            {
                this.DataControl.CssClass += " col-sm-6";
                this.CurrencyControl.CssClass += " col-sm-6";
                this.DataControlParent.Controls.Add(this.CurrencyControl);
                if (currency != null)
                    this.CurrencyControl.SelectedValue = Convert.ToString(currency.GetPropertyValue(this.CurrencyControl.ValueMemberName));
                else
                    this.CurrencyControl.SelectedValue = "-1";
            }
            else
            {
                this.DataControl.CssClass += " col-sm-6";
                var label = new Label();
                label.CssClass = "currency-label col-sm-6";
                label.Text = Convert.ToString(currency.GetPropertyValue(this.CurrencyControl.DisplayMemberName));
                this.DataControlParent.Controls.Add(label);
            }


            if (this.IsRequired)
            {
                this.CurrencyControl.CssClass += " required";
                this.CurrencyControl.AddAttribute("aria-required", "true");
            }

            this.HasValue = this.DataControl.Value.IsNumeric() && this.DataControl.Value.ToDecimal() > 0;
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
        protected string FormatValue(object value)
        {
            if (value == null)
                return "";
            if (value is string)
            {
                if (value.ToString().IndexOf(".") > -1 || value.ToString().IndexOf(",") > -1)
                {
                    value = Convert.ToDecimal(value);
                }
            }
            if (!string.IsNullOrEmpty(this.Format))
            {
                var decimalValue = Convert.ToDecimal(value);
                return decimalValue.ToString(this.Format);
            }
            return value.ToString();
        }
        public MoneyField(FieldContainer<T> FieldContainer) : base(FieldContainer)
        {
            this.CurrencyControl = new Select();
        }
    }
}
