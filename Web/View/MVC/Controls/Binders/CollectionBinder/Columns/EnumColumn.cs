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
    public class EnumColumn<TModel, T> : BaseColumn<TModel, T> where T : class where TModel : ListModel<T>
    {
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

            var selectbox = new UI.Controls.Select();
            selectbox.ID = this.IdentifierKeyword + identifierValue;
            selectbox.Name = selectbox.ID;
            selectbox.AddAttribute("data-identifier", Convert.ToString(identifierValue));
            selectbox.AddAttribute("data-column", this.FormatColumnName());

            selectbox.CssClass = "form-control";
            selectbox.DataSource = this.EnumType.GetEnumSelectList(this.Binder.Client);
            selectbox.DefaultText = this.Binder.Client.TranslateText("Select");
            selectbox.DefaultValue = "-1";
            selectbox.DisplayMemberName = "Text";
            selectbox.ValueMemberName = "Value";
            if (value != null)
                selectbox.SelectedValue = Convert.ToString(value);
            return selectbox;
        }
        public EnumColumn(CollectionBinder<TModel, T> binder, string Name) : base(binder, Name)
        {

        }
    }
}
