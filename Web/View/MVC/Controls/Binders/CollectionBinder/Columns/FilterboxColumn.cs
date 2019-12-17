using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ophelia.Web.UI.Controls;

using System.Linq.Expressions;
using Ophelia.Web.View.Mvc.Models;
using Ophelia.Reflection;
using System.Collections;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ophelia.Web.View.Mvc.Controls.Binders.CollectionBinder.Columns
{
    public class FilterboxColumn<TModel, T> : BaseColumn<TModel, T> where T : class where TModel : ListModel<T>
    {
        public bool IsMultiple { get; set; }
        public Expression<Func<T, object>> SelectedValueExpression { get; set; }
        public object SelectedValue { get; set; }
        public string DisplayMember { get; set; }
        public Func<T, object> DisplayMemberExpression { get; set; }
        public string AlternateDisplayMember { get; set; }
        public string ValueMember { get; set; }
        public string AjaxURL { get; set; }
        public override object GetValue(T item)
        {
            if (item != null)
                return this.GetItem(item, item).Text;
            return base.GetValue(item);
        }

        public override WebControl GetEditableControl(T entity, object value)
        {
            var identifierValue = this.IdentifierExpression.GetValue(entity);

            var control = new Ophelia.Web.UI.Controls.Select();
            control.ID = this.IdentifierKeyword + identifierValue;
            control.Name = control.ID;
            control.AddAttribute("data-identifier", Convert.ToString(identifierValue));
            control.AddAttribute("data-column", this.FormatColumnName());

            if (!this.IsMultiple)
                control.CssClass = "filterbox single-value select-remote-data select2-hidden-accessible";
            else
            {
                control.CssClass = "filterbox multiple-value select2-hidden-accessible";
                control.AddAttribute("multiple", "multiple");
            }

            if (!string.IsNullOrEmpty(this.ValueMember) && (!string.IsNullOrEmpty(this.DisplayMember) || this.DisplayMemberExpression != null) && this.SelectedValueExpression != null)
            {
                this.SelectedValue = this.SelectedValueExpression.GetValue(entity);
                if (this.SelectedValue != null)
                {
                    var list = new List<SelectListItem>();
                    if (this.SelectedValue is IEnumerable)
                    {
                        var datalist = (IEnumerable)this.SelectedValue;
                        foreach (var item in datalist)
                        {
                            list.Add(this.GetItem(item, entity));
                        }
                    }
                    else
                    {
                        list.Add(this.GetItem(this.SelectedValue, entity));
                    }
                    control.DataSource = list;
                    control.SelectedValue = string.Join(",", list.Select(op => op.Value).ToArray());
                }
            }
            else
            {
                var list = new List<SelectListItem>();
                if (this.SelectedValue != null)
                {
                    if (this.SelectedValue is SelectListItem)
                    {
                        list.Add((SelectListItem)this.SelectedValue);
                    }
                    else if (this.SelectedValue.GetType().IsPrimitive)
                    {
                        list.Add(new SelectListItem() { Value = Convert.ToString(this.SelectedValue), Text = Convert.ToString(this.SelectedValue) });
                    }
                    else if (this.SelectedValue is IEnumerable)
                    {
                        var datalist = (IEnumerable)this.SelectedValue;
                        foreach (var item in datalist)
                        {
                            list.Add(this.GetItem(item, entity));
                        }
                    }
                    if (list.Count > 0)
                        list.FirstOrDefault().Selected = true;
                }
                control.DataSource = list;
                control.SelectedValue = string.Join(",", list.Select(op => op.Value).ToArray());
            }
            control.DisplayMemberName = "Text";
            control.ValueMemberName = "Value";
            control.AddAttribute("data-clear", "true");

            if (!string.IsNullOrEmpty(this.AjaxURL))
                control.AddAttribute("data-url", this.AjaxURL);
            return control;
        }
        private SelectListItem GetItem(object item, T entity)
        {
            if (item == entity)
            {
                if (this.SelectedValueExpression != null)
                {
                    item = this.SelectedValueExpression.GetValue(entity);
                }
            }
            var accessor = new Accessor();
            accessor.Item = item;
            var id = "";
            if (this.ValueMember != "ValueItself")
            {
                accessor.MemberName = this.ValueMember;
                id = Convert.ToString(accessor.Value);
            }
            else
                id = Convert.ToString(item);
            var name = "";
            if (this.DisplayMemberExpression == null)
            {
                if (!string.IsNullOrEmpty(this.AlternateDisplayMember))
                {
                    accessor.MemberName = "";
                    if (item.GetType().GetProperty(this.DisplayMember) != null)
                        accessor.MemberName = this.DisplayMember;
                    else if (this.DisplayMember.IndexOf(".") > -1)
                        accessor.MemberName = this.DisplayMember;
                    if (string.IsNullOrEmpty(accessor.MemberName))
                        accessor.MemberName = this.AlternateDisplayMember;
                }
                else
                    accessor.MemberName = this.DisplayMember;
                name = Convert.ToString(accessor.Value);
            }
            else
            {
                name = (string)this.DisplayMemberExpression.Execute(entity);
            }
            return new SelectListItem() { Selected = true, Text = name, Value = id };
        }
        public FilterboxColumn(CollectionBinder<TModel, T> binder, string Name) : base(binder, Name)
        {

        }
    }
}