using Ophelia.Web.UI.Controls;
using Ophelia.Web.View.Mvc.Controls.Binders.Fields;
using Ophelia.Web.View.Mvc.Models;
using System;

namespace Ophelia.Web.View.Mvc.Controls.Binders.CollectionBinder.Columns
{
    public class DateColumn<TModel, T> : BaseColumn<TModel, T> where T : class where TModel : ListModel<T>
    {
        public DateTimeFormatType Format { get; set; }
        public DateFieldMode Mode { get; set; }
        public string DateType { get; set; }
        public string TimeType { get; set; }
        public override object GetValue(T item)
        {
            var value = base.GetValue(item);
            if (value != null)
            {
                DateTime dateValue = DateTime.MinValue;
                if (DateTime.TryParse(Convert.ToString(value), out dateValue))
                {
                    if (dateValue == DateTime.MinValue)
                        return "";

                    if (this.Format == DateTimeFormatType.DateOnly)
                        return dateValue.ToString("dd.MM.yyyy");
                    if (this.Format == DateTimeFormatType.DateTimeWithHour)
                        return dateValue.ToString("dd.MM.yyyy HH:mm");
                    if (this.Format == DateTimeFormatType.TimeOnly)
                        return dateValue.ToString("HH:mm");
                }
            }
            return value;
        }
        public WebControl GetEditableControl(T entity, object value, object value2)
        {
            if (this.Mode == DateFieldMode.DoubleSelection)
            {
                var panel = new Panel();
                panel.Style.Add("position", "relative;");
                panel.CssClass = "date-field-container";

                if (BinderConfiguration.UseHtml5DataTypes)
                    this.DateType = "date";

                var DataControl = new Textbox();
                DataControl.Name = this.FormatName() + "Low";
                DataControl.ID = DataControl.Name;
                DataControl.AddAttribute("data-column", this.FormatColumnName());
                DataControl.CssClass = "form-control date-field pickadate-selectors";
                if (value != null)
                    DataControl.Value = this.FormatValue(Convert.ToDateTime(value));
                
                if (this.Format == DateTimeFormatType.TimeOnly)
                    DataControl.Type = this.TimeType;
                else
                {
                    if (this.DateType == "date")
                        DataControl.CssClass = "form-control date-field";
                    DataControl.Type = this.DateType;
                }
                panel.Controls.Add(DataControl);

                var SecondDataControl = new Textbox();
                SecondDataControl.Name = this.FormatName() + "High";
                SecondDataControl.ID = SecondDataControl.Name;
                SecondDataControl.AddAttribute("data-column", this.FormatColumnName());
                SecondDataControl.CssClass = "form-control date-field pickadate-selectors";
                if (value2 != null)
                    SecondDataControl.Value = this.FormatValue(Convert.ToDateTime(value2));

                if (this.Format == DateTimeFormatType.TimeOnly)
                    SecondDataControl.Type = this.TimeType;
                else
                {
                    if (this.DateType == "date")
                        SecondDataControl.CssClass = "form-control date-field";
                    SecondDataControl.Type = this.DateType;
                }
                panel.Controls.Add(SecondDataControl);

                SecondDataControl.AddAttribute("placeholder", this.Binder.Client.TranslateText("EndDate"));
                DataControl.AddAttribute("placeholder", this.Binder.Client.TranslateText("StartDate"));

                return panel;
            }
            else
            {
                var control = (Textbox)base.GetEditableControl(entity, value);
                if (this.Format == DateTimeFormatType.TimeOnly)
                {
                    control.Type = "time";
                }
                return control;
            }
        }
        private string FormatValue(DateTime value)
        {
            if (value == DateTime.MinValue)
                return "";
            
            if (this.Format == DateTimeFormatType.DateOnly || this.Format == DateTimeFormatType.DateTimeWithHour)
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
            return "";
        }
        public override WebControl GetEditableControl(T entity, object value)
        {
            return GetEditableControl(entity, value, null);
        }
        public DateColumn(CollectionBinder<TModel, T> binder, string Name) : base(binder, Name)
        {
            this.Format = DateTimeFormatType.DateTimeWithHour;
            this.DateType = "text";
            this.TimeType = "time";
            if (BinderConfiguration.UseHtml5DataTypes)
                this.DateType = "date";
        }
    }
}
