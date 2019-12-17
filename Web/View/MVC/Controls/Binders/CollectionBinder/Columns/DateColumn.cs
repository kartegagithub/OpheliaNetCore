using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ophelia.Web.UI.Controls;

using System.Linq.Expressions;
using Ophelia.Web.View.Mvc.Models;
using Ophelia.Web.View.Mvc.Controls.Binders.Fields;

namespace Ophelia.Web.View.Mvc.Controls.Binders.CollectionBinder.Columns
{
    public class DateColumn<TModel, T> : BaseColumn<TModel, T> where T : class where TModel : ListModel<T>
    {
        public DateTimeFormatType Format { get; set; }
        public DateFieldMode Mode { get; set; }
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
        public override WebControl GetEditableControl(T entity, object value)
        {
            if(this.Mode == DateFieldMode.DoubleSelection)
            {
                var panel = new Panel();
                panel.Style.Add("position", "relative;");

                var DataControl = new Textbox();
                DataControl.Name = this.FormatName() + "Low";
                DataControl.ID = DataControl.Name;
                DataControl.AddAttribute("data-column", this.FormatColumnName());
                DataControl.CssClass = "form-control date-field pickadate-selectors";
                if (this.Format == DateTimeFormatType.TimeOnly)
                    DataControl.Type = "time";
                else
                {
                    if (BinderConfiguration.UseHtml5DataTypes)
                    {
                        DataControl.CssClass = "form-control";
                        DataControl.Type = "date";
                    }
                }
                panel.Controls.Add(DataControl);

                var SecondDataControl = new Textbox();
                SecondDataControl.Name = this.FormatName() + "High";
                SecondDataControl.ID = SecondDataControl.Name;
                SecondDataControl.AddAttribute("data-column", this.FormatColumnName());
                SecondDataControl.CssClass = "form-control date-field pickadate-selectors";
                if (this.Format == DateTimeFormatType.TimeOnly)
                    SecondDataControl.Type = "time";
                else
                {
                    if (BinderConfiguration.UseHtml5DataTypes)
                    {
                        SecondDataControl.CssClass = "form-control";
                        SecondDataControl.Type = "date";
                    }
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
        public DateColumn(CollectionBinder<TModel, T> binder, string Name) : base(binder, Name)
        {
            this.Format = DateTimeFormatType.DateTimeWithHour;
        }
    }
}
