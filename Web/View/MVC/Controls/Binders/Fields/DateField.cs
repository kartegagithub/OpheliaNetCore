using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ophelia.Web.UI.Controls;
using Ophelia;

using System.Linq.Expressions;

namespace Ophelia.Web.View.Mvc.Controls.Binders.Fields
{
    public class DateField<T> : BaseField<T> where T : class
    {
        public DateTimeFormatType Format { get; set; }
        public DateFieldMode Mode { get; set; }
        public new Textbox DataControl { get { return (Textbox)base.DataControl; } set { base.DataControl = value; } }
        public Expression<Func<T, object>> LowExpression { get; set; }
        public string LowPropertyName { get; set; }
        public object LowExpressionValue { get; set; }
        public Expression<Func<T, object>> HighExpression { get; set; }
        public string HighPropertyName { get; set; }
        public object HighExpressionValue { get; set; }
        protected override WebControl CreateDataControl()
        {
            return new Textbox();
        }
        protected override void onBeforeRenderControl(TextWriter writer)
        {
            base.onBeforeRenderControl(writer);
            this.DataControl.CssClass = "form-control date-field pickadate-selectors";
            if (BinderConfiguration.UseHtml5DataTypes)
            {
                this.DataControl.CssClass = "form-control";
                this.DataControl.Type = "date";
            }
            if (this.Mode == DateFieldMode.SingleSelection)
            {
                if (this.ExpressionValue != null)
                {
                    this.DataControl.Value = this.FormatValue(Convert.ToDateTime(this.ExpressionValue));
                }
                if(this.Format == DateTimeFormatType.DateTimeWithHour)
                {
                    var SecondDataControl = new Textbox();
                    SecondDataControl.CssClass = "form-control time";
                    this.DataControlParent.Controls.Add(SecondDataControl);
                    SecondDataControl.Name = this.DataControl.Name + "-Time";
                    SecondDataControl.Type = "time";
                    SecondDataControl.Style["width"] = "100px";
                    SecondDataControl.Style["display"] = "inline-block";
                    SecondDataControl.Style["margin-left"] = "10px";
                    SecondDataControl.Value = Convert.ToDateTime(this.ExpressionValue).ToString("HH:mm");
                    SecondDataControl.ID = SecondDataControl.Name;

                    if (Convert.ToDateTime(this.ExpressionValue) > DateTime.MinValue)
                    {
                        var tmpFormat = this.Format;
                        this.Format = DateTimeFormatType.DateOnly;
                        this.DataControl.Value = this.FormatValue(Convert.ToDateTime(this.ExpressionValue));
                        this.Format = tmpFormat;
                    }
                    this.DataControl.Style["width"] = "100px";
                    this.DataControl.Style["display"] = "inline-block";
                }
                else if (this.Format == DateTimeFormatType.TimeOnly)
                {
                    this.DataControl.Type = "time";
                    this.DataControl.CssClass = "form-control time";
                }
            }
            else if (this.Mode == DateFieldMode.DoubleSelection)
            {
                var SecondDataControl = new Textbox();
                this.DataControl.CssClass += " date-range-low";
                SecondDataControl.CssClass = "form-control date-field pickadate-selectors date-range-high";
                if (BinderConfiguration.UseHtml5DataTypes)
                {
                    SecondDataControl.CssClass = "form-control";
                    SecondDataControl.Type = "date";
                }
                this.DataControlParent.Controls.Add(SecondDataControl);
                if (this.HighExpression != null && this.HighExpressionValue == null)
                {
                    SecondDataControl.Name = this.HighExpression.Body.ParsePath();
                    SecondDataControl.ID = SecondDataControl.Name;
                    this.HighExpressionValue = this.HighExpression.GetValue(this.FieldContainer.Entity, this.FieldContainer.CurrentLanguageID, this.FieldContainer.DefaultEntityProperties);
                    if (this.HighExpressionValue.IsDate() && Convert.ToDateTime(this.HighExpressionValue) > DateTime.MinValue)
                        SecondDataControl.Value = this.FormatValue(Convert.ToDateTime(this.HighExpressionValue));
                }
                else if (!string.IsNullOrEmpty(this.HighPropertyName))
                {
                    SecondDataControl.Name = this.HighPropertyName;
                    SecondDataControl.ID = SecondDataControl.Name;
                    if(this.HighExpressionValue.IsDate() && Convert.ToDateTime(this.HighExpressionValue) > DateTime.MinValue)
                        SecondDataControl.Value = this.FormatValue(Convert.ToDateTime(this.HighExpressionValue));
                }
                if (this.LowExpression != null && this.LowExpressionValue == null)
                {
                    this.DataControl.Name = this.LowExpression.Body.ParsePath();
                    this.DataControl.ID = this.DataControl.Name;
                    this.LowExpressionValue = this.LowExpression.GetValue(this.FieldContainer.Entity, this.FieldContainer.CurrentLanguageID, this.FieldContainer.DefaultEntityProperties);
                    if (this.LowExpressionValue.IsDate() && Convert.ToDateTime(this.LowExpressionValue) > DateTime.MinValue)
                        this.DataControl.Value = this.FormatValue(Convert.ToDateTime(this.LowExpressionValue));
                }
                else if (!string.IsNullOrEmpty(this.LowPropertyName))
                {
                    this.DataControl.Name = this.LowPropertyName;
                    this.DataControl.ID = this.DataControl.Name;
                    if (this.LowExpressionValue.IsDate() && Convert.ToDateTime(this.LowExpressionValue) > DateTime.MinValue)
                        this.DataControl.Value = this.FormatValue(Convert.ToDateTime(this.LowExpressionValue));
                }
                this.HasValue = !string.IsNullOrEmpty(this.DataControl.Value) || !string.IsNullOrEmpty(SecondDataControl.Value);
                if (string.IsNullOrEmpty(this.Text))
                {
                    var name = SecondDataControl.Name.Left(SecondDataControl.Name.Length - 4);
                    if (name.IndexOf(".") > -1)
                        name = name.Split('.')[1];
                    this.LabelControl.Text = this.Client.TranslateText(name);
                }
                SecondDataControl.AddAttribute("placeholder", this.FieldContainer.Client.TranslateText("EndDate"));
                this.DataControl.AddAttribute("placeholder", this.FieldContainer.Client.TranslateText("StartDate"));
            }
        }
        private string FormatValue(DateTime value)
        {
            if(value == DateTime.MinValue)
            {
                return "";
            }
            if (this.Format == DateTimeFormatType.DateOnly)
            {
                if (BinderConfiguration.UseHtml5DataTypes)
                    return value.ToString("yyyy-MM-dd");
                else
                    return value.ToString("dd.MM.yyyy");
            }
            else if (this.Format == DateTimeFormatType.TimeOnly)
            {
                return value.ToString("HH:mm");
            }
            if (BinderConfiguration.UseHtml5DataTypes)
                return value.ToString("yyyy-MM-dd HH:mm");
            else
                return value.ToString("dd.MM.yyyy HH:mm");
        }
        public DateField(FieldContainer<T> FieldContainer) :base(FieldContainer)
        {
            this.Format = DateTimeFormatType.DateOnly;
            this.Mode = DateFieldMode.SingleSelection;
        }
    }
    public enum DateFieldMode
    {
        SingleSelection,
        DoubleSelection
    }
}
