using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.ComponentModel.DataAnnotations;

using System.Reflection;
using Ophelia.Web.View.Mvc.Models;
using System.Web;
using Ophelia.Reflection;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ophelia.Web.View.Mvc.Controls.Binders.CollectionBinder
{
    public static class Extensions
    {
        public static Columns.TextColumn<TModel, T> AddTextColumn<TModel, T>(this CollectionBinder<TModel, T> binder, Expression<Func<T, object>> expression) where T : class where TModel : ListModel<T> { return binder.AddTextColumn(expression, ""); }
        public static Columns.TextColumn<TModel, T> AddTextColumn<TModel, T>(this CollectionBinder<TModel, T> binder, Expression<Func<T, object>> expression, string Text) where T : class where TModel : ListModel<T> { return binder.AddTextColumn(expression, Text, true); }
        public static Columns.TextColumn<TModel, T> AddTextColumn<TModel, T>(this CollectionBinder<TModel, T> binder, Expression<Func<T, object>> expression, string Text, bool IsSortable) where T : class where TModel : ListModel<T> { return binder.AddTextColumn(expression, Text, IsSortable, false); }
        public static Columns.TextColumn<TModel, T> AddTextColumn<TModel, T>(this CollectionBinder<TModel, T> binder, Expression<Func<T, object>> expression, string Text, bool IsSortable, bool IsHidden) where T : class where TModel : ListModel<T>
        {
            var column = new Columns.TextColumn<TModel, T>(binder, Text);
            column.Expression = expression;
            column.Text = Text;
            column.IsSortable = IsSortable;
            column.IsHidden = IsHidden;
            return (Columns.TextColumn<TModel, T>)binder.AddColumn(column);
        }

        public static Columns.TextColumn<TModel, T> AddEditableTextColumn<TModel, T>(this CollectionBinder<TModel, T> binder, Expression<Func<T, object>> expression, Expression<Func<T, object>> identifierExpression, string identifierKeyword = "", string Text = "", object htmlAttributes = null) where T : class where TModel : ListModel<T>
        {
            var column = new Columns.TextColumn<TModel, T>(binder, Text);
            column.Expression = expression;
            column.IdentifierExpression = identifierExpression;
            column.IdentifierKeyword = identifierKeyword;
            column.HtmlAttributes = htmlAttributes;
            column.Text = Text;
            column.AllowEdit = true;
            return (Columns.TextColumn<TModel, T>)binder.AddColumn(column);
        }

        public static Columns.DateColumn<TModel, T> AddEditableDateColumn<TModel, T>(this CollectionBinder<TModel, T> binder, Expression<Func<T, object>> expression, Expression<Func<T, object>> identifierExpression, string identifierKeyword = "", string Text = "", object htmlAttributes = null) where T : class where TModel : ListModel<T>
        {
            var column = new Columns.DateColumn<TModel, T>(binder, Text);
            column.Expression = expression;
            column.IdentifierExpression = identifierExpression;
            column.IdentifierKeyword = identifierKeyword;
            column.HtmlAttributes = htmlAttributes;
            column.Text = Text;
            column.AllowEdit = true;
            return (Columns.DateColumn<TModel, T>)binder.AddColumn(column);
        }
        public static Columns.DateColumn<TModel, T> AddDateColumn<TModel, T>(this CollectionBinder<TModel, T> binder, Expression<Func<T, object>> expression) where T : class where TModel : ListModel<T>
        {
            var column = new Columns.DateColumn<TModel, T>(binder, "");
            column.Expression = expression;
            return (Columns.DateColumn<TModel, T>)binder.AddColumn(column);
        }
        public static Columns.DateColumn<TModel, T> AddDateColumn<TModel, T>(this CollectionBinder<TModel, T> binder, Expression<Func<T, object>> expression, string Text) where T : class where TModel : ListModel<T>
        {
            var column = new Columns.DateColumn<TModel, T>(binder, "");
            column.Expression = expression;
            column.Text = Text;
            return (Columns.DateColumn<TModel, T>)binder.AddColumn(column);
        }

        public static Columns.BoolColumn<TModel, T> AddBoolColumn<TModel, T>(this CollectionBinder<TModel, T> binder, Expression<Func<T, object>> expression) where T : class where TModel : ListModel<T>
        {
            var column = new Columns.BoolColumn<TModel, T>(binder, "");
            column.Expression = expression;
            return (Columns.BoolColumn<TModel, T>)binder.AddColumn(column);
        }
        public static Columns.BoolColumn<TModel, T> AddBoolColumn<TModel, T>(this CollectionBinder<TModel, T> binder, Expression<Func<T, object>> expression, string Text) where T : class where TModel : ListModel<T>
        {
            var column = new Columns.BoolColumn<TModel, T>(binder, "");
            column.Expression = expression;
            column.Text = Text;
            return (Columns.BoolColumn<TModel, T>)binder.AddColumn(column);
        }
        public static Columns.BoolColumn<TModel, T> AddEditableBoolColumn<TModel, T>(this CollectionBinder<TModel, T> binder, Expression<Func<T, object>> identifierExpression, Func<T, object> comparisonFunction, bool singleSelection = true, string identifierKeyword = "", object htmlAttributes = null) where T : class where TModel : ListModel<T>
        {
            return AddEditableBoolColumn(binder, null, identifierExpression, comparisonFunction, singleSelection, identifierKeyword, htmlAttributes);
        }
        public static Columns.BoolColumn<TModel, T> AddEditableBoolColumn<TModel, T>(this CollectionBinder<TModel, T> binder, Expression<Func<T, object>> expression, Expression<Func<T, object>> identifierExpression, Func<T, object> comparisonFunction, bool singleSelection = true, string identifierKeyword = "", object htmlAttributes = null) where T : class where TModel : ListModel<T>
        {
            var column = new Columns.BoolColumn<TModel, T>(binder, "");
            column.Expression = expression;
            column.IdentifierExpression = identifierExpression;
            column.HtmlAttributes = htmlAttributes;
            column.ComparisonFunction = comparisonFunction;
            column.IdentifierKeyword = identifierKeyword;
            column.SingleSelection = singleSelection;
            column.AllowEdit = true;
            return (Columns.BoolColumn<TModel, T>)binder.AddColumn(column);
        }
        public static Columns.EnumColumn<TModel, T> AddEditableEnumColumn<TModel, T>(this CollectionBinder<TModel, T> binder, Expression<Func<T, object>> expression, Type enumType, Expression<Func<T, object>> identifierExpression, string identifierKeyword = "", object htmlAttributes = null) where T : class where TModel : ListModel<T>
        {
            var column = new Columns.EnumColumn<TModel, T>(binder, "");
            column.Expression = expression;
            column.EnumType = enumType;
            column.AllowEdit = true;
            column.IdentifierExpression = identifierExpression;
            column.HtmlAttributes = htmlAttributes;
            column.IdentifierKeyword = identifierKeyword;
            return (Columns.EnumColumn<TModel, T>)binder.AddColumn(column);
        }
        public static Columns.ImageColumn<TModel, T> AddImageColumn<TModel, T>(this CollectionBinder<TModel, T> binder, Expression<Func<T, object>> expression, object htmlAttributes = null) where T : class where TModel : ListModel<T>
        {
            var column = new Columns.ImageColumn<TModel, T>(binder, "");
            column.Expression = expression;
            column.HtmlAttributes = htmlAttributes;
            return (Columns.ImageColumn<TModel, T>)binder.AddColumn(column);
        }

        public static Columns.EnumColumn<TModel, T> AddEnumColumn<TModel, T>(this CollectionBinder<TModel, T> binder, Expression<Func<T, object>> expression, Type enumType) where T : class where TModel : ListModel<T>
        {
            var column = new Columns.EnumColumn<TModel, T>(binder, "");
            column.Expression = expression;
            column.EnumType = enumType;
            return (Columns.EnumColumn<TModel, T>)binder.AddColumn(column);
        }
        public static Columns.EnumColumn<TModel, T> AddEnumColumn<TModel, T>(this CollectionBinder<TModel, T> binder, Expression<Func<T, object>> expression, Type enumType, string Text) where T : class where TModel : ListModel<T>
        {
            var column = new Columns.EnumColumn<TModel, T>(binder, "");
            column.Expression = expression;
            column.EnumType = enumType;
            column.Text = Text;
            return (Columns.EnumColumn<TModel, T>)binder.AddColumn(column);
        }

        public static Columns.TextColumn<TModel, T> AddEditableNumericColumn<TModel, T>(this CollectionBinder<TModel, T> binder, Expression<Func<T, object>> expression, Expression<Func<T, object>> identifierExpression, string identifierKeyword = "", string Text = "", object htmlAttributes = null) where T : class where TModel : ListModel<T>
        {
            var column = new Columns.NumericColumn<TModel, T>(binder, Text);
            column.Expression = expression;
            column.IdentifierExpression = identifierExpression;
            column.IdentifierKeyword = identifierKeyword;
            column.HtmlAttributes = htmlAttributes;
            column.Text = Text;
            column.AllowEdit = true;
            return (Columns.TextColumn<TModel, T>)binder.AddColumn(column);
        }
        public static Columns.NumericColumn<TModel, T> AddNumericColumn<TModel, T>(this CollectionBinder<TModel, T> binder, Expression<Func<T, object>> expression) where T : class where TModel : ListModel<T> { return binder.AddNumericColumn(expression, ""); }
        public static Columns.NumericColumn<TModel, T> AddNumericColumn<TModel, T>(this CollectionBinder<TModel, T> binder, Expression<Func<T, object>> expression, string Text) where T : class where TModel : ListModel<T> { return binder.AddNumericColumn(expression, Text, true); }
        public static Columns.NumericColumn<TModel, T> AddNumericColumn<TModel, T>(this CollectionBinder<TModel, T> binder, Expression<Func<T, object>> expression, string Text, bool IsSortable) where T : class where TModel : ListModel<T> { return binder.AddNumericColumn(expression, Text, IsSortable, false); }
        public static Columns.NumericColumn<TModel, T> AddNumericColumn<TModel, T>(this CollectionBinder<TModel, T> binder, Expression<Func<T, object>> expression, string Text, bool IsSortable, bool IsHidden) where T : class where TModel : ListModel<T>
        {
            var column = new Columns.NumericColumn<TModel, T>(binder, Text);
            column.Expression = expression;
            column.Text = Text;
            column.IsSortable = IsSortable;
            column.IsHidden = IsHidden;
            return (Columns.NumericColumn<TModel, T>)binder.AddColumn(column);
        }

        public static Columns.FilterboxColumn<TModel, T> AddFilterboxColumn<TModel, T>(this CollectionBinder<TModel, T> binder, string ajaxURL, Expression<Func<T, object>> expression, Expression<Func<T, object>> selectedValueExpression, string valueMember = "ID", string DisplayMember = "Name", bool isRequired = false, object htmlAttributes = null, bool IsMultiple = false) where T : class where TModel : ListModel<T> { return (Columns.FilterboxColumn<TModel, T>)binder.AddFilterboxColumn("", ajaxURL, expression, selectedValueExpression, valueMember, DisplayMember, isRequired, htmlAttributes, IsMultiple); }
        public static Columns.FilterboxColumn<TModel, T> AddFilterboxColumn<TModel, T>(this CollectionBinder<TModel, T> binder, string Text, string ajaxURL, Expression<Func<T, object>> expression, Expression<Func<T, object>> selectedValueExpression, string valueMember = "ID", string DisplayMember = "Name", bool isRequired = false, object htmlAttributes = null, bool IsMultiple = false) where T : class where TModel : ListModel<T>
        {
            var control = new Columns.FilterboxColumn<TModel, T>(binder, Text);
            control.Expression = expression;
            control.SelectedValueExpression = selectedValueExpression;
            control.ValueMember = valueMember;
            control.DisplayMember = DisplayMember;
            control.Text = Text;
            control.HtmlAttributes = htmlAttributes;
            control.AjaxURL = ajaxURL;
            control.IsMultiple = IsMultiple;
            return (Columns.FilterboxColumn<TModel, T>)binder.AddColumn(control);
        }

        public static Columns.FilterboxColumn<TModel, T> AddFilterboxColumn<TModel, T>(this CollectionBinder<TModel, T> binder, string Text, string ajaxURL, string name, SelectListItem value, bool isRequired = false, object htmlAttributes = null) where T : class where TModel : ListModel<T>
        {
            var control = new Columns.FilterboxColumn<TModel, T>(binder, Text);
            control.SelectedValue = value;
            control.Text = Text;
            control.HtmlAttributes = htmlAttributes;
            control.AjaxURL = ajaxURL;
            return (Columns.FilterboxColumn<TModel, T>)binder.AddColumn(control);
        }
        public static Columns.FilterboxColumn<TModel, T> AddEditableFilterboxColumn<TModel, T>(this CollectionBinder<TModel, T> binder, string ajaxURL, Expression<Func<T, object>> expression, Expression<Func<T, object>> selectedValueExpression, Expression<Func<T, object>> identifierExpression, string identifierKeyword = "", string valueMember = "ID", string DisplayMember = "Name", bool isRequired = false, object htmlAttributes = null, bool IsMultiple = false) where T : class where TModel : ListModel<T> { return (Columns.FilterboxColumn<TModel, T>)binder.AddEditableFilterboxColumn("", ajaxURL, expression, selectedValueExpression, identifierExpression, identifierKeyword, valueMember, DisplayMember, isRequired, htmlAttributes, IsMultiple); }
        public static Columns.FilterboxColumn<TModel, T> AddEditableFilterboxColumn<TModel, T>(this CollectionBinder<TModel, T> binder, string Text, string ajaxURL, Expression<Func<T, object>> expression, Expression<Func<T, object>> selectedValueExpression, Expression<Func<T, object>> identifierExpression, string identifierKeyword = "", string valueMember = "ID", string DisplayMember = "Name", bool isRequired = false, object htmlAttributes = null, bool IsMultiple = false) where T : class where TModel : ListModel<T>
        {
            var control = new Columns.FilterboxColumn<TModel, T>(binder, Text);
            control.Expression = expression;
            control.SelectedValueExpression = selectedValueExpression;
            control.IdentifierExpression = identifierExpression;
            control.IdentifierKeyword = identifierKeyword;
            control.ValueMember = valueMember;
            control.DisplayMember = DisplayMember;
            control.Text = Text;
            control.HtmlAttributes = htmlAttributes;
            control.AjaxURL = ajaxURL;
            control.IsMultiple = IsMultiple;
            control.AllowEdit = true;
            return (Columns.FilterboxColumn<TModel, T>)binder.AddColumn(control);
        }

        //public static Columns.SelectboxColumn<TModel, T> AddSelectboxColumn<TModel, T>(this CollectionBinder<TModel, T> binder, Expression<Func<T, object>> expression, IEnumerable dataSource, bool isRequired = false, object htmlAttributes = null, string DefaultText = "", string DefaultValue = "") where T : class where TModel : ListModel<T> { return (Columns.SelectboxColumn<TModel, T>)binder.AddSelectboxColumn("", expression, dataSource, isRequired, htmlAttributes, DefaultText, DefaultValue); }
        //public static Columns.SelectboxColumn<TModel, T> AddSelectboxColumn<TModel, T>(this CollectionBinder<TModel, T> binder, string Text, Expression<Func<T, object>> expression, IEnumerable dataSource, bool isRequired = false, object htmlAttributes = null, string DefaultText = "", string DefaultValue = "") where T : class where TModel : ListModel<T>
        //{
        //    var control = new Columns.SelectboxColumn<TModel, T>(binder, Text);
        //    control.Expression = expression;
        //    control.DataControl.DataSource = dataSource;
        //    control.DataControl.DefaultText = DefaultText;
        //    control.DataControl.DefaultValue = DefaultValue;
        //    control.Text = Text;
        //    control.IsRequired = isRequired;
        //    control.ID = Text;
        //    container.AddField(control);
        //    container.SetFieldProperties(control);
        //    return (Columns.SelectboxColumn<TModel, T>)binder.AddColumn(control);
        //}
        public static Columns.BaseColumn<TModel, T> AddColumn<TModel, T>(this CollectionBinder<TModel, T> binder, Columns.BaseColumn<TModel, T> column) where T : class where TModel : ListModel<T>
        {
            column.Visible = binder.CanAddColumn(column);
            binder.Columns.Add(column);
            return column;
        }
    }
}
