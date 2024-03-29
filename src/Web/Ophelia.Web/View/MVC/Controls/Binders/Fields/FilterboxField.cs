﻿using Microsoft.AspNetCore.Mvc.Rendering;
using Ophelia.Reflection;
using Ophelia.Web.UI.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace Ophelia.Web.View.Mvc.Controls.Binders.Fields
{
    public class FilterboxField<T> : SelectboxField<T> where T : class
    {
        public Expression<Func<T, object>> SelectedValueExpression { get; set; }
        public object SelectedValue { get; set; }
        public string DisplayMember { get; set; }
        public Func<T, object> DisplayMemberExpression { get; set; }
        public string AlternateDisplayMember { get; set; }
        public string ValueMember { get; set; }
        public string AjaxURL { get; set; }

        public new Select DataControl { get { return base.DataControl; } set { base.DataControl = value; } }

        protected override void onBeforeRenderControl(TextWriter writer)
        {
            base.onBeforeRenderControl(writer);
            if (!this.DataControl.IsMultiple)
                this.DataControl.CssClass += " filterbox single-value select-remote-data select2-hidden-accessible";
            else
            {
                this.DataControl.CssClass += " filterbox multiple-value select2-hidden-accessible";
                this.DataControl.AddAttribute("multiple", "multiple");
            }

            if (!string.IsNullOrEmpty(this.ValueMember) && (!string.IsNullOrEmpty(this.DisplayMember) || this.DisplayMemberExpression != null) && this.SelectedValueExpression != null)
            {
                this.SelectedValue = this.SelectedValueExpression.GetValue(this.FieldContainer.Entity);
                this.FieldContainer.ValidateSelectedValue(this);
                if (this.SelectedValue != null)
                {
                    var list = new List<SelectListItem>();
                    if (!(this.SelectedValue is string) && this.SelectedValue is IEnumerable)
                    {
                        var datalist = (IEnumerable)this.SelectedValue;
                        foreach (var item in datalist)
                        {
                            list.Add(this.GetItem(item));
                        }
                    }
                    else
                    {
                        list.Add(this.GetItem(this.SelectedValue));
                    }
                    this.DataControl.DataSource = list;
                    this.DataControl.SelectedValue = string.Join(",", list.Select(op => op.Value).ToArray());

                    var entityType = this.SelectedValueExpression.GetPropertyType();
                    if (entityType != null)
                    {
                        this.DataControl.AddAttribute("view-namespace", entityType.GetNamespace());
                        this.DataControl.AddAttribute("view-type", entityType.Name);
                    }
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
                    else if (this.SelectedValue is string || this.SelectedValue.GetType().IsPrimitive)
                    {
                        list.Add(new SelectListItem() { Value = Convert.ToString(this.SelectedValue), Text = Convert.ToString(this.SelectedValue) });
                    }
                    else if (this.SelectedValue is IEnumerable)
                    {
                        var datalist = (IEnumerable)this.SelectedValue;
                        foreach (var item in datalist)
                        {
                            list.Add(this.GetItem(item));
                        }
                    }
                    else
                    {
                        list.Add(this.GetItem(this.SelectedValue));
                    }
                    if (list.Count > 0)
                        list.FirstOrDefault().Selected = true;
                }
                this.DataControl.DataSource = list;
                this.DataControl.SelectedValue = string.Join(",", list.Select(op => op.Value).ToArray());
            }
            this.DataControl.DisplayMemberName = "Text";
            this.DataControl.ValueMemberName = "Value";
            this.DataControl.AddAttribute("data-clear", "true");

            if (!string.IsNullOrEmpty(this.AjaxURL))
                this.DataControl.AddAttribute("data-url", this.AjaxURL);

            if (this.SelectedValue != null && this.SelectedValue is SelectListItem)
                this.HasValue = !string.IsNullOrEmpty((this.SelectedValue as SelectListItem).Value) && !(this.SelectedValue as SelectListItem).Value.Equals("0");
            else if (SelectedValue != null && SelectedValue is ICollection)
                this.HasValue = (SelectedValue as ICollection).Count > 0;
            else
                this.HasValue = this.SelectedValue != null && !this.SelectedValue.Equals(0);

            if (this.IsRequired)
            {
                this.DataControl.CssClass += " required";
                this.DataControl.AddAttribute("aria-required", "true");
            }
            if (this.SearchHelp != null)
            {
                this.DataControl.AddAttribute("data-search-help", "1");
                this.DataControl.AddAttribute("data-search-help-url", this.SearchHelp.URL);
                this.DataControl.AddAttribute("data-search-help-callback", this.SearchHelp.Callback);
                if (this.SearchHelp.Props != null)
                {
                    var type = this.SearchHelp.Props.GetType();
                    var props = type.GetProperties().ToDictionary(op => op.Name, op => op.GetValue(this.SearchHelp.Props, null));
                    foreach (var item in props)
                    {
                        this.DataControl.AddAttribute("data-search-help-" + item.Key, Convert.ToString(item.Value));
                    }
                }
            }
        }

        private SelectListItem GetItem(object item)
        {
            if (item != null && (this.SelectedValue is string || item.GetType().IsPrimitive))
            {
                return new SelectListItem() { Selected = true, Text = Convert.ToString(item), Value = Convert.ToString(item) };
            }
            else if (item != null)
            {
                var accessor = new Accessor();
                accessor.Item = item;

                accessor.MemberName = this.ValueMember;
                var id = Convert.ToString(accessor.Value);
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
                    name = (string)this.DisplayMemberExpression.Execute(this.FieldContainer.Entity);
                }
                return new SelectListItem() { Selected = true, Text = name, Value = id };
            }
            else
                return new SelectListItem();
        }
        public FilterboxField(FieldContainer<T> FieldContainer) : base(FieldContainer)
        {
            this.DisplayMember = "ID";
            this.ValueMember = "Name";
            this.AlternateDisplayMember = "Title";
        }
    }
}
