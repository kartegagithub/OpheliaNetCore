using Ophelia.Web.UI.Controls;
using System;
using System.IO;
using System.Linq.Expressions;

namespace Ophelia.Web.View.Mvc.Controls.Binders.Fields
{
    public class NumberboxField<T> : TextboxField<T> where T : class
    {
        public NumberboxFieldMode Mode { get; set; }
        public new Textbox DataControl { get { return (Textbox)base.DataControl; } set { base.DataControl = value; } }
        public Expression<Func<T, object>> LowExpression { get; set; }
        public string LowPropertyName { get; set; }
        public string Format { get; set; }
        public object LowExpressionValue { get; set; }
        public Expression<Func<T, object>> HighExpression { get; set; }
        public string HighPropertyName { get; set; }
        public object HighExpressionValue { get; set; }

        public string LowPlaceHolder { get; set; }
        public string HighPlaceHolder { get; set; }

        protected override WebControl CreateDataControl()
        {
            return new Textbox();
        }
        protected override void onBeforeRenderControl(TextWriter writer)
        {
            base.onBeforeRenderControl(writer);
            this.DataControl.CssClass += " numeric";
            this.DataControl.Value = this.FormatValue(this.DataControl.Value);
            if (BinderConfiguration.UseHtml5DataTypes)
                this.DataControl.Type = "number";

            if (this.Mode == NumberboxFieldMode.SingleSelection)
            {
                this.HasValue = this.DataControl.Value.IsNumeric() && this.DataControl.Value.ToInt64() > 0;
            }
            else if (this.Mode == NumberboxFieldMode.DoubleSelection)
            {
                var SecondDataControl = new Textbox();
                this.DataControl.CssClass += " numberbox-low";
                SecondDataControl.CssClass = "form-control numeric numberbox-high";
                this.DataControlParent.Controls.Add(SecondDataControl);
                if (this.HighExpression != null && this.HighExpressionValue == null)
                {
                    SecondDataControl.Name = this.HighExpression.Body.ParsePath();
                    SecondDataControl.ID = SecondDataControl.Name;
                    this.HighExpressionValue = this.HighExpression.GetValue(this.FieldContainer.Entity, this.FieldContainer.CurrentLanguageID, this.FieldContainer.DefaultEntityProperties);
                    if (this.HighExpressionValue != null && Convert.ToInt64(this.HighExpressionValue) > 0)
                        SecondDataControl.Value = this.FormatValue(this.HighExpressionValue);
                }
                else if (!string.IsNullOrEmpty(this.HighPropertyName))
                {
                    SecondDataControl.Name = this.HighPropertyName;
                    SecondDataControl.ID = SecondDataControl.Name;
                    if (this.HighExpressionValue != null && Convert.ToInt64(this.HighExpressionValue) > 0)
                        SecondDataControl.Value = this.FormatValue(this.HighExpressionValue);
                }
                if (this.LowExpression != null && this.LowExpressionValue == null)
                {
                    this.DataControl.Name = this.LowExpression.Body.ParsePath();
                    this.DataControl.ID = this.DataControl.Name;
                    this.LowExpressionValue = this.LowExpression.GetValue(this.FieldContainer.Entity, this.FieldContainer.CurrentLanguageID, this.FieldContainer.DefaultEntityProperties);
                    if (this.LowExpressionValue != null && Convert.ToInt64(this.LowExpressionValue) > 0)
                        this.DataControl.Value = this.FormatValue(this.LowExpressionValue);
                }
                else if (!string.IsNullOrEmpty(this.LowPropertyName))
                {
                    this.DataControl.Name = this.LowPropertyName;
                    this.DataControl.ID = this.DataControl.Name;
                    if (this.LowExpressionValue != null && Convert.ToInt64(this.LowExpressionValue) > 0)
                        this.DataControl.Value = this.FormatValue(this.LowExpressionValue);
                }
                this.HasValue = (!string.IsNullOrEmpty(this.DataControl.Value) && this.DataControl.Value.ToInt64() > 0) || (!string.IsNullOrEmpty(SecondDataControl.Value) && SecondDataControl.Value.ToInt64() > 0);
                if (string.IsNullOrEmpty(this.Text))
                {
                    var name = SecondDataControl.Name.Left(SecondDataControl.Name.Length - 4);
                    if (name.IndexOf(".") > -1)
                        name = name.Split('.')[1];
                    this.LabelControl.Text = this.Client.TranslateText(name);
                }

                SecondDataControl.AddAttribute("placeholder", this.FieldContainer.Client.TranslateText(this.HighPlaceHolder));
                this.DataControl.AddAttribute("placeholder", this.FieldContainer.Client.TranslateText(this.LowPlaceHolder));
            }
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
        public NumberboxField(FieldContainer<T> FieldContainer) : base(FieldContainer)
        {
            this.Mode = NumberboxFieldMode.SingleSelection;
        }
    }
    public enum NumberboxFieldMode
    {
        SingleSelection,
        DoubleSelection
    }
}
