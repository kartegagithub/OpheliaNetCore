using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ophelia.Web.UI.Controls;

using System.Linq.Expressions;
using Ophelia.Web.View.Mvc.Models;
using System.Collections;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ophelia.Web.View.Mvc.Controls.Binders.CollectionBinder.Columns
{
    public class BoolColumn<TModel, T> : BaseColumn<TModel, T> where T:class where TModel : ListModel<T>
    {
        public bool SingleSelection { get; set; }
        public bool CanSetValue { get; set; }
        public string DataControlCssClass { get; set; }
        public IEnumerable<SelectListItem> Datasource { get; set; }
        public Func<T, object> ComparisonFunction { get; set; }
        public override object GetValue(T item)
        {
            if (item != null)
            {
                var value = base.GetValue(item);
                if (value != null)
                {
                    var convertedValue = false;
                    if (bool.TryParse(value.ToString(), out convertedValue))
                    {
                        return this.Binder.Client.TranslateText(convertedValue.ToString());
                    }
                }
                return value;
            }
            return "";
        }

        public override WebControl GetEditableControl(T entity, object value)
        {
            var orginalValue = value;
            var identifierValue = this.IdentifierExpression.GetValue(entity);
            if (this.Datasource == null)
            {
                if (this.SingleSelection)
                {
                    var control = new Radio();
                    control.HtmlAttributes = this.HtmlAttributes;
                    control.ID = this.IdentifierKeyword + identifierValue;
                    control.Name = this.IdentifierKeyword;
                    control.AddAttribute("data-identifier", Convert.ToString(identifierValue));
                    control.AddAttribute("data-column", this.FormatColumnName());
                    if (this.CanSetValue)
                    {
                        if (!string.IsNullOrEmpty(Convert.ToString(value)))
                            control.Value = Convert.ToString(value);
                        else if (!string.IsNullOrEmpty(Convert.ToString(identifierValue)))
                            control.Value = Convert.ToString(identifierValue);
                    }
                    control.CssClass = this.DataControlCssClass;
                    if (this.ComparisonFunction != null && entity != null)
                        control.Checked = Convert.ToBoolean(this.ComparisonFunction.Execute(entity));
                    return control;
                }
                else
                {
                    var control = new Checkbox();
                    control.HtmlAttributes = this.HtmlAttributes;
                    control.ID = this.IdentifierKeyword + identifierValue;
                    control.Name = control.ID;
                    control.AddAttribute("data-identifier", Convert.ToString(identifierValue));
                    control.AddAttribute("data-column", this.FormatColumnName());
                    if (this.CanSetValue)
                    {
                        if (!string.IsNullOrEmpty(Convert.ToString(value)))
                            control.Value = Convert.ToString(value);
                        else if (!string.IsNullOrEmpty(Convert.ToString(identifierValue)))
                            control.Value = Convert.ToString(identifierValue);
                    }
                    control.CssClass = this.DataControlCssClass;
                    if (this.ComparisonFunction != null && entity != null)
                        control.Checked = Convert.ToBoolean(this.ComparisonFunction.Execute(entity));
                    return control;
                }
            }
            else
            {
                var selectbox = new UI.Controls.Select();
                selectbox.ID = this.IdentifierKeyword + identifierValue;
                selectbox.Name = selectbox.ID;
                selectbox.AddAttribute("data-identifier", Convert.ToString(identifierValue));
                selectbox.AddAttribute("data-column", this.FormatColumnName());

                selectbox.CssClass = "form-control";
                selectbox.DataSource = this.Datasource;
                selectbox.DefaultText = this.Binder.Client.TranslateText("Select");
                selectbox.DefaultValue = "-1";
                selectbox.DisplayMemberName = "Text";
                selectbox.ValueMemberName = "Value";
                if (value != null)
                    selectbox.SelectedValue = Convert.ToString(value);
                return selectbox;
            }
        }

        public WebControl GetEditableControlAsSelect(T entity, object value)
        {
            var list = new List<SelectListItem>();
            list.Add(new SelectListItem() { Text = this.Binder.Client.TranslateText("True"), Value = "1" });
            list.Add(new SelectListItem() { Text = this.Binder.Client.TranslateText("False"), Value = "0" });
            this.Datasource = list;
            return this.GetEditableControl(entity, value);
        }
        public BoolColumn(CollectionBinder<TModel, T> binder, string Name) : base(binder, Name)
        {
            this.CanSetValue = true;
        }
    }
}