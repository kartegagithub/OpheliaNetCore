using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ophelia.Web.UI.Controls;

using System.Linq.Expressions;
using Ophelia.Web.View.Mvc.Models;

namespace Ophelia.Web.View.Mvc.Controls.Binders.CollectionBinder.Columns
{
    public class NumericColumn<TModel, T> : TextColumn<TModel, T> where T:class where TModel : ListModel<T>
    {
        public string Format { get; set; }
        public override object GetValue(T item)
        {
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
            textbox.Value = Convert.ToString(this.GetValue(entity));
            this.SetAttributes(textbox);
            return textbox;
        }
        public NumericColumn(CollectionBinder<TModel,T> binder, string Name) : base(binder, Name)
        {
            this.Alignment = HorizontalAlign.Right;
        }
    }
}
