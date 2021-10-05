using Ophelia.Web.UI.Controls;
using Ophelia.Web.View.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Ophelia.Web.View.Mvc.Controls.Binders.CollectionBinder.Columns
{
    public abstract class BaseColumn<TModel, T> where T : class where TModel : ListModel<T>
    {
        private bool _Visible = false;

        public CollectionBinder<TModel, T> Binder { get; private set; }
        public Expression<Func<T, object>> Expression { get; set; }
        public string IdentifierKeyword { get; set; }
        public object HtmlAttributes { get; set; }
        public Expression<Func<T, object>> IdentifierExpression { get; set; }
        public bool IsSortable { get; set; }
        public string Width { get; set; }
        public bool AllowEdit { get; set; }
        public bool IsHidden { get; set; }
        public bool AllowSum { get; set; }
        public bool CanTranslateText { get; set; }
        public int MaxTextLength { get; set; }
        public string Text { get; set; }
        public string Name { get; set; }
        public int SortOrder { get; set; }
        public bool KeepHtml { get; set; }
        public bool EnableColumnFiltering { get; set; }
        public bool EnableGrouping { get; set; }
        private string formattedName = "";
        private string formattedColumnName = "";
        public bool Visible
        {
            get { return _Visible; }
            set
            {
                _Visible = value || _Visible;
            }
        }
        public bool HideColumnTitle { get; set; }
        public HorizontalAlign Alignment { get; set; }

        public string FormatName()
        {
            if (!string.IsNullOrEmpty(this.formattedName))
                return this.formattedName;
            if (!string.IsNullOrEmpty(this.Name))
            {
                this.formattedName = this.Name;
            }
            else if (this.Expression != null)
            {
                var path = this.Expression.ParsePath();
                if (path.IndexOf("(") > -1)
                    path = path.Replace("(", "").Replace(")", "");
                if (path.IndexOf(".") > -1)
                {
                    var tmp = path.Split('.');
                    path = tmp[tmp.Length - 2] + "ID";
                }
                this.formattedName = path;
            }
            return this.formattedName;
        }
        public string FormatColumnName()
        {
            if (!string.IsNullOrEmpty(this.formattedColumnName))
                return this.formattedColumnName;
            if (this.Expression != null)
            {
                var path = this.Expression.ParsePath();
                if (path.IndexOf("(") > -1)
                    path = path.Replace("(", "").Replace(")", "");
                //if (path.IndexOf(".") > -1)
                //{
                //    var tmp = path.Split('.');
                //    path = tmp[tmp.Length - 2] + "ID";
                //}
                this.formattedColumnName = path;
            }
            else if (!string.IsNullOrEmpty(this.Name))
            {
                this.formattedColumnName = this.Name;
            }
            return this.formattedColumnName;
        }
        public string FormatText()
        {
            if (!string.IsNullOrEmpty(this.Text))
            {
                if (this.CanTranslateText)
                    return this.Binder.Client.TranslateText(this.Text);
                else
                    return Text;
            }
            else if (this.Expression != null)
            {
                return this.Binder.Client.TranslateText(this.FormatName());
            }
            return "";
        }
        public virtual WebControl GetEditableControl(T entity, object value)
        {
            var identifierValue = this.IdentifierExpression.GetValue(entity);

            var textbox = new Textbox();
            textbox.ID = this.IdentifierKeyword + identifierValue;
            textbox.AddAttribute("data-identifier", Convert.ToString(identifierValue));
            textbox.AddAttribute("data-column", this.FormatColumnName());
            textbox.Name = textbox.ID;
            textbox.HtmlAttributes = this.HtmlAttributes;
            textbox.CssClass = "form-control";
            textbox.Value = Convert.ToString(this.GetValue(entity));

            if (this is Columns.NumericColumn<TModel, T> && textbox.Value == "0")
                textbox.Value = "";

            if (string.IsNullOrEmpty(textbox.Value) && value != null)
            {
                try
                {
                    textbox.Value = value.ToString();
                }
                catch { }
            }
            this.SetAttributes(textbox);
            return textbox;
        }
        protected void SetAttributes(WebControl dataControl)
        {
            if (this.HtmlAttributes != null)
            {
                var type = this.HtmlAttributes.GetType();
                var props = type.GetProperties().ToDictionary(op => op.Name, op => op.GetValue(this.HtmlAttributes, null));
                var newProps = new Dictionary<string, object>();
                var dataControlProps = new Dictionary<string, object>();
                foreach (var item in props)
                {
                    if (item.Key.StartsWith("datacontrol_"))
                    {
                        var key = item.Key.Replace("datacontrol_", "");
                        if (key == "class")
                        {
                            dataControl.CssClass += " " + item.Value;
                        }
                        else
                        {
                            dataControl.AddAttribute(key, Convert.ToString(item.Value));
                        }
                    }
                }
            }
        }
        public virtual object GetValue(T item)
        {
            object value = null;
            if (this.Expression != null)
            {
                value = this.Expression.GetValue(item, this.Binder.Client.CurrentLanguageID, this.Binder.DefaultEntityProperties, new Reflection.Accessor.NullReferenceEventDelegate(this.NullReferenceEventHandler));
            }
            return value;
        }
        protected void NullReferenceEventHandler(object item, System.Reflection.PropertyInfo propInfo)
        {
            this.Binder.HandleColumnValueNullReference(item, propInfo);
        }
        public BaseColumn(CollectionBinder<TModel, T> binder, string Name)
        {
            this.Binder = binder;
            this.Name = Name;
            this.Alignment = HorizontalAlign.Left;
            this.CanTranslateText = true;
            this.EnableColumnFiltering = true;
            this.EnableGrouping = true;
            this.IsSortable = true;
        }
    }
}
