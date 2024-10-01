using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Ophelia.Web.View.Mvc.Controls.Binders
{
    public static class FieldExtensions
    {
        public static Fields.HiddenField<T> AddHiddenField<T>(this FieldContainer<T> container, string Name, Expression<Func<T, object>> expression) where T : class
        {
            var control = new Fields.HiddenField<T>(container);
            control.Expression = expression;
            control.DataControl.ID = Name;
            control.DataControl.Name = Name;
            return (Fields.HiddenField<T>)container.AddField(control);
        }

        public static Fields.HiddenField<T> AddHiddenField<T>(this FieldContainer<T> container, Expression<Func<T, object>> expression) where T : class
        {
            var control = new Fields.HiddenField<T>(container);
            control.DataControl.ID = expression.ParsePath();
            control.DataControl.Name = control.DataControl.ID;
            return (Fields.HiddenField<T>)container.AddField(control);
        }

        public static Fields.HiddenField<T> AddHiddenField<T>(this FieldContainer<T> container, string Name, object Value) where T : class
        {
            var control = new Fields.HiddenField<T>(container);
            control.DataControl.Value = Convert.ToString(Value);
            control.DataControl.ID = Name;
            control.DataControl.Name = Name;
            control.ID = Name;
            return (Fields.HiddenField<T>)container.AddField(control);
        }

        public static Fields.TextboxField<T> AddTextboxField<T>(this FieldContainer<T> container, string name, bool isRequired) where T : class { return container.AddTextboxField(name, "", isRequired); }
        public static Fields.TextboxField<T> AddTextboxField<T>(this FieldContainer<T> container, string name, string value, bool isRequired) where T : class { return container.AddTextboxField(name, value, isRequired, null); }
        public static Fields.TextboxField<T> AddTextboxField<T>(this FieldContainer<T> container, string name, string value, bool isRequired, object htmlAttributes) where T : class { return container.AddTextboxField(name, name, value, isRequired, htmlAttributes); }
        public static Fields.TextboxField<T> AddTextboxField<T>(this FieldContainer<T> container, Expression<Func<T, object>> expression, bool isRequired = false, object htmlAttributes = null) where T : class { return container.AddTextboxField("", expression, isRequired, htmlAttributes); }
        public static Fields.TextboxField<T> AddTextboxField<T>(this FieldContainer<T> container, string Text, string name, string value, bool isRequired, object htmlAttributes) where T : class
        {
            var control = new Fields.TextboxField<T>(container);
            control.DataControl.ID = name;
            control.DataControl.Name = name;
            control.DataControl.Value = value;
            control.HtmlAttributes = htmlAttributes;
            control.IsRequired = isRequired;
            control.Text = Text;
            control.ID = name;
            return (Fields.TextboxField<T>)container.AddField(control);
        }
        public static Fields.TextboxField<T> AddTextboxField<T>(this FieldContainer<T> container, string Text, Expression<Func<T, object>> expression, bool isRequired = false, object htmlAttributes = null) where T : class
        {
            var control = new Fields.TextboxField<T>(container);
            control.Expression = expression;
            control.Text = Text;
            control.ID = Text;
            control.IsRequired = isRequired;
            control.HtmlAttributes = htmlAttributes;
            return (Fields.TextboxField<T>)container.AddField(control);
        }

        public static Fields.NumberboxField<T> AddNumberboxField<T>(this FieldContainer<T> container, Expression<Func<T, object>> expression, bool isRequired = false, object htmlAttributes = null) where T : class { return container.AddNumberboxField("", expression, isRequired, htmlAttributes); }
        public static Fields.NumberboxField<T> AddNumberboxField<T>(this FieldContainer<T> container, string Text, Expression<Func<T, object>> expression, bool isRequired = false, object htmlAttributes = null) where T : class
        {
            var control = new Fields.NumberboxField<T>(container);
            control.Expression = expression;
            control.Text = Text;
            control.DataControl.ID = Text;
            control.DataControl.Name = Text;
            control.ID = Text;
            control.IsRequired = isRequired;
            control.HtmlAttributes = htmlAttributes;
            return (Fields.NumberboxField<T>)container.AddField(control);
        }
        public static Fields.NumberboxField<T> AddNumberboxField<T>(this FieldContainer<T> container, string Text, string name, string value, bool isRequired = false, object htmlAttributes = null) where T : class
        {
            var control = new Fields.NumberboxField<T>(container);
            control.DataControl.ID = name;
            control.DataControl.Name = name;
            control.DataControl.Value = value;
            control.Text = Text;
            control.ID = name;
            control.IsRequired = isRequired;
            control.HtmlAttributes = htmlAttributes;
            return (Fields.NumberboxField<T>)container.AddField(control);
        }

        public static Fields.NumberboxField<T> AddNumberboxRangeField<T>(this FieldContainer<T> container, Expression<Func<T, object>> expressionLow, Expression<Func<T, object>> expressionHigh, bool isRequired = false, object htmlAttributes = null) where T : class { return container.AddNumberboxRangeField("", expressionLow, expressionHigh, isRequired, htmlAttributes); }
        public static Fields.NumberboxField<T> AddNumberboxRangeField<T>(this FieldContainer<T> container, string Text, Expression<Func<T, object>> expressionLow, Expression<Func<T, object>> expressionHigh, bool isRequired = false, object htmlAttributes = null, string HighPlaceHolder = "End", string LowPlaceHolder = "Start") where T : class
        {
            var control = new Fields.NumberboxField<T>(container);
            control.HighExpression = expressionHigh;
            control.HighPropertyName = expressionHigh.ParsePath();
            control.LowExpression = expressionLow;
            control.LowPropertyName = expressionLow.ParsePath();
            control.LowPlaceHolder = LowPlaceHolder;
            control.HighPlaceHolder = HighPlaceHolder;
            control.Mode = Fields.NumberboxFieldMode.DoubleSelection;
            control.Text = Text;
            control.ID = Text;
            control.IsRequired = isRequired;
            control.HtmlAttributes = htmlAttributes;
            return (Fields.NumberboxField<T>)container.AddField(control);
        }
        public static Fields.NumberboxField<T> AddNumberboxRangeField<T>(this FieldContainer<T> container, string Text, string LowProperty, string HighProperty, object LowPropertyValue, object HighPropertyValue, bool isRequired = false, object htmlAttributes = null, string HighPlaceHolder = "End", string LowPlaceHolder = "Start") where T : class
        {
            var control = new Fields.NumberboxField<T>(container);
            control.Text = Text;
            control.LowPropertyName = LowProperty;
            control.HighPropertyName = HighProperty;
            control.LowPlaceHolder = LowPlaceHolder;
            control.HighPlaceHolder = HighPlaceHolder;
            control.LowExpressionValue = LowPropertyValue;
            control.HighExpressionValue = HighPropertyValue;
            control.Mode = Fields.NumberboxFieldMode.DoubleSelection;
            control.IsRequired = isRequired;
            control.HtmlAttributes = htmlAttributes;
            return (Fields.NumberboxField<T>)container.AddField(control);
        }

        public static Fields.PasswordField<T> AddPasswordField<T>(this FieldContainer<T> container, Expression<Func<T, object>> expression, bool isRequired = false, object htmlAttributes = null) where T : class { return container.AddPasswordField("", expression, isRequired, htmlAttributes); }
        public static Fields.PasswordField<T> AddPasswordField<T>(this FieldContainer<T> container, string Text, Expression<Func<T, object>> expression, bool isRequired = false, object htmlAttributes = null) where T : class
        {
            var control = new Fields.PasswordField<T>(container);
            control.Expression = expression;
            control.Text = Text;
            control.ID = Text;
            control.IsRequired = isRequired;
            control.HtmlAttributes = htmlAttributes;
            return (Fields.PasswordField<T>)container.AddField(control);
        }

        public static Fields.DateField<T> AddDateRangeField<T>(this FieldContainer<T> container, Expression<Func<T, object>> expressionLow, Expression<Func<T, object>> expressionHigh, bool isRequired = false, object htmlAttributes = null) where T : class { return container.AddDateRangeField("", expressionLow, expressionHigh, isRequired, htmlAttributes); }
        public static Fields.DateField<T> AddDateRangeField<T>(this FieldContainer<T> container, string Text, Expression<Func<T, object>> expressionLow, Expression<Func<T, object>> expressionHigh, bool isRequired = false, object htmlAttributes = null) where T : class
        {
            var control = new Fields.DateField<T>(container);
            control.HighExpression = expressionHigh;
            control.HighPropertyName = expressionHigh.ParsePath();
            control.LowExpression = expressionLow;
            control.LowPropertyName = expressionLow.ParsePath();
            control.Mode = Fields.DateFieldMode.DoubleSelection;
            control.Text = Text;
            control.ID = Text;
            control.IsRequired = isRequired;
            control.HtmlAttributes = htmlAttributes;
            return (Fields.DateField<T>)container.AddField(control);
        }
        public static Fields.DateField<T> AddDateRangeField<T>(this FieldContainer<T> container, string Text, string LowProperty, string HighProperty, object LowPropertyValue, object HighPropertyValue, bool isRequired = false, object htmlAttributes = null) where T : class
        {
            var control = new Fields.DateField<T>(container);
            control.Text = Text;
            control.LowPropertyName = LowProperty;
            control.HighPropertyName = HighProperty;
            control.LowExpressionValue = LowPropertyValue;
            control.HighExpressionValue = HighPropertyValue;
            control.Mode = Fields.DateFieldMode.DoubleSelection;
            control.IsRequired = isRequired;
            control.HtmlAttributes = htmlAttributes;
            return (Fields.DateField<T>)container.AddField(control);
        }
        public static Fields.DateField<T> AddDateField<T>(this FieldContainer<T> container, Expression<Func<T, object>> expression, DateTimeFormatType format = DateTimeFormatType.DateOnly, bool isRequired = false, object htmlAttributes = null, bool IsReadonly = false) where T : class
        {
            return container.AddDateField("", expression, format, isRequired, htmlAttributes, IsReadonly);
        }
        public static Fields.DateField<T> AddDateField<T>(this FieldContainer<T> container, string Text, Expression<Func<T, object>> expression, DateTimeFormatType format = DateTimeFormatType.DateOnly, bool isRequired = false, object htmlAttributes = null, bool IsReadonly = false) where T : class
        {
            var control = new Fields.DateField<T>(container);
            control.Expression = expression;
            control.Text = Text;
            control.IsRequired = isRequired;
            control.Format = format;
            control.Editable = !IsReadonly;
            control.HtmlAttributes = htmlAttributes;
            control.ID = Text;
            container.AddField(control);
            return (Fields.DateField<T>)container.AddField(control);
        }
        public static Fields.FilterboxField<T> AddFilterboxField<T>(this FieldContainer<T> container, string ajaxURL, Expression<Func<T, object>> expression, Expression<Func<T, object>> selectedValueExpression, string valueMember = "ID", string DisplayMember = "Name", bool isRequired = false, object htmlAttributes = null, bool IsMultiple = false) where T : class { return container.AddFilterboxField("", ajaxURL, expression, selectedValueExpression, valueMember, DisplayMember, isRequired, htmlAttributes, IsMultiple); }
        public static Fields.FilterboxField<T> AddFilterboxField<T>(this FieldContainer<T> container, string Text, string ajaxURL, Expression<Func<T, object>> expression, Expression<Func<T, object>> selectedValueExpression, string valueMember = "ID", string DisplayMember = "Name", bool isRequired = false, object htmlAttributes = null, bool IsMultiple = false) where T : class
        {
            var control = new Fields.FilterboxField<T>(container);
            control.Expression = expression;
            control.SelectedValueExpression = selectedValueExpression;
            control.ValueMember = valueMember;
            control.DisplayMember = DisplayMember;
            control.Text = Text;
            control.IsRequired = isRequired;
            control.HtmlAttributes = htmlAttributes;
            control.AjaxURL = ajaxURL;
            control.DataControl.IsMultiple = IsMultiple;
            control.ID = Text;
            container.AddField(control);
            container.SetFieldProperties(control);
            return control;
        }

        public static Fields.FilterboxField<T> AddFilterboxField<T>(this FieldContainer<T> container, string Text, string ajaxURL, string name, Microsoft.AspNetCore.Mvc.Rendering.SelectListItem value, bool isRequired = false, object htmlAttributes = null) where T : class
        {
            var control = new Fields.FilterboxField<T>(container);
            control.DataControl.ID = name;
            control.DataControl.Name = name;
            control.SelectedValue = value;
            control.Text = Text;
            control.IsRequired = isRequired;
            control.HtmlAttributes = htmlAttributes;
            control.AjaxURL = ajaxURL;
            control.ID = Text;
            container.AddField(control);
            container.SetFieldProperties(control);
            return control;
        }

        public static Fields.RichTextField<T> AddRichTextField<T>(this FieldContainer<T> container, Expression<Func<T, object>> expression, bool isRequired = false, object htmlAttributes = null) where T : class { return container.AddRichTextField("", expression, isRequired, htmlAttributes); }
        public static Fields.RichTextField<T> AddRichTextField<T>(this FieldContainer<T> container, string Text, Expression<Func<T, object>> expression, bool isRequired = false, object htmlAttributes = null) where T : class
        {
            var control = new Fields.RichTextField<T>(container);
            control.Expression = expression;
            control.Text = Text;
            control.IsRequired = isRequired;
            control.ID = Text;
            control.HtmlAttributes = htmlAttributes;
            return (Fields.RichTextField<T>)container.AddField(control);
        }

        public static Fields.CheckboxField<T> AddCheckboxField<T>(this FieldContainer<T> container, Expression<Func<T, object>> expression, bool isRequired = false, object htmlAttributes = null, string DataOnText = "Yes", string DataOffText = "No") where T : class { return container.AddCheckboxField("", expression, isRequired, htmlAttributes, DataOnText, DataOffText); }
        public static Fields.CheckboxField<T> AddCheckboxField<T>(this FieldContainer<T> container, string Text, Expression<Func<T, object>> expression, bool isRequired = false, object htmlAttributes = null, string DataOnText = "Yes", string DataOffText = "No") where T : class
        {
            var control = new Fields.CheckboxField<T>(container);
            control.Expression = expression;
            control.Text = Text;
            control.IsRequired = isRequired;
            control.DataControl.DataOnText = DataOnText;
            control.DataControl.DataOffText = DataOffText;
            control.ID = Text;
            control.HtmlAttributes = htmlAttributes;
            return (Fields.CheckboxField<T>)container.AddField(control);
        }

        public static Fields.CheckboxField<T> AddCheckboxField<T>(this FieldContainer<T> container, string Text, string Name, bool value, bool isRequired = false, object htmlAttributes = null, string DataOnText = "Yes", string DataOffText = "No") where T : class
        {
            var control = new Fields.CheckboxField<T>(container);
            control.DataControl.Checked = value;
            control.DataControl.ID = Name;
            control.DataControl.Name = Name;
            control.Text = Text;
            control.IsRequired = isRequired;
            control.DataControl.DataOnText = DataOnText;
            control.DataControl.DataOffText = DataOffText;
            control.ID = Text;
            control.HtmlAttributes = htmlAttributes;
            return (Fields.CheckboxField<T>)container.AddField(control);
        }

        public static Fields.RadioboxField<T> AddRadioboxField<T>(this FieldContainer<T> container, Expression<Func<T, object>> expression, bool isRequired = false, object htmlAttributes = null) where T : class { return container.AddRadioboxField("", expression, isRequired, htmlAttributes); }
        public static Fields.RadioboxField<T> AddRadioboxField<T>(this FieldContainer<T> container, string Text, Expression<Func<T, object>> expression, bool isRequired = false, object htmlAttributes = null) where T : class
        {
            var control = new Fields.RadioboxField<T>(container);
            control.Expression = expression;
            control.Text = Text;
            control.IsRequired = isRequired;
            control.ID = Text;
            control.HtmlAttributes = htmlAttributes;
            return (Fields.RadioboxField<T>)container.AddField(control);
        }
        public static Fields.SelectboxField<T> AddBoolSelectboxField<T>(this FieldContainer<T> container, Expression<Func<T, object>> expression, bool isRequired = false, object htmlAttributes = null, string DefaultText = "", string DefaultValue = "") where T : class
        {
            var list = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
            if (string.IsNullOrEmpty(DefaultText))
                list.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Text = container.Client.TranslateText("Select"), Value = "-1" });

            list.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Text = container.Client.TranslateText("True"), Value = "1" });
            list.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Text = container.Client.TranslateText("False"), Value = "0" });
            return container.AddSelectboxField("", expression, list, isRequired, htmlAttributes, DefaultText, DefaultValue);
        }

        public static Fields.SelectboxField<T> AddSelectboxField<T>(this FieldContainer<T> container, Expression<Func<T, object>> expression, IEnumerable dataSource, bool isRequired = false, object htmlAttributes = null, string DefaultText = "", string DefaultValue = "") where T : class { return container.AddSelectboxField("", expression, dataSource, isRequired, htmlAttributes, DefaultText, DefaultValue); }
        public static Fields.SelectboxField<T> AddSelectboxField<T>(this FieldContainer<T> container, string Text, Expression<Func<T, object>> expression, IEnumerable dataSource, bool isRequired = false, object htmlAttributes = null, string DefaultText = "", string DefaultValue = "") where T : class
        {
            var control = new Fields.SelectboxField<T>(container);
            control.Expression = expression;
            control.DataControl.DataSource = dataSource;
            control.DataControl.DefaultText = DefaultText;
            control.DataControl.DefaultValue = DefaultValue;
            control.Text = Text;
            control.IsRequired = isRequired;
            control.ID = Text;
            control.HtmlAttributes = htmlAttributes;
            container.AddField(control);
            container.SetFieldProperties(control);
            return control;
        }
        public static Fields.SelectboxField<T> AddSelectboxField<T>(this FieldContainer<T> container, string Text, string Name, string defaultValue, IEnumerable dataSource, bool isRequired = false, object htmlAttributes = null, string DefaultText = "", string DefaultValue = "", string DisplayMemberName = "Name", string ValueMemberName = "ID") where T : class
        {
            var control = new Fields.SelectboxField<T>(container);
            control.Name = Name;
            control.DataControl.ID = Name;
            control.DataControl.Name = Name;
            control.DataControl.SelectedValue = defaultValue;
            control.DataControl.DataSource = dataSource;
            control.DataControl.DefaultText = DefaultText;
            control.DataControl.DefaultValue = DefaultValue;
            control.DataControl.DisplayMemberName = DisplayMemberName;
            control.DataControl.ValueMemberName = ValueMemberName;
            control.HtmlAttributes = htmlAttributes;
            control.Text = Text;
            control.IsRequired = isRequired;
            control.HtmlAttributes = htmlAttributes;
            container.AddField(control);
            container.SetFieldProperties(control);
            return control;
        }

        public static Fields.SelectboxField<T> AddEnumSelectboxField<T>(this FieldContainer<T> container, Expression<Func<T, object>> expression, Type enumType, bool isRequired = false, object htmlAttributes = null, string DefaultText = "", string DefaultValue = "", bool isMultiple = false) where T : class { return container.AddEnumSelectboxField("", expression, enumType, isRequired, htmlAttributes, DefaultText, DefaultValue, isMultiple); }
        public static Fields.SelectboxField<T> AddEnumSelectboxField<T>(this FieldContainer<T> container, string Text, Expression<Func<T, object>> expression, Type enumType, bool isRequired = false, object htmlAttributes = null, string DefaultText = "", string DefaultValue = "", bool isMultiple = false) where T : class
        {
            var control = new Fields.SelectboxField<T>(container);
            control.Expression = expression;
            control.Text = Text;
            control.DataControl.DataSource = enumType.GetEnumSelectList(container.Client);
            control.DataControl.DefaultText = DefaultText;
            control.DataControl.DefaultValue = DefaultValue;
            control.DataControl.DisplayMemberName = "Text";
            control.DataControl.ValueMemberName = "Value";
            control.DataControl.IsMultiple = isMultiple;
            control.IsRequired = isRequired;
            control.HtmlAttributes = htmlAttributes;
            return (Fields.SelectboxField<T>)container.AddField(control);
        }

        public static Fields.LabelField<T> AddLabelField<T>(this FieldContainer<T> container, Expression<Func<T, object>> expression, object htmlAttributes = null) where T : class { return container.AddLabelField("", expression, htmlAttributes); }
        public static Fields.LabelField<T> AddLabelField<T>(this FieldContainer<T> container, string Text, Expression<Func<T, object>> expression, object htmlAttributes = null) where T : class
        {
            var control = new Fields.LabelField<T>(container);
            control.Expression = expression;
            control.Text = Text;
            control.HtmlAttributes = htmlAttributes;
            return (Fields.LabelField<T>)container.AddField(control);
        }
        public static Fields.LabelField<T> AddLabelField<T>(this FieldContainer<T> container, string Text, string value, object htmlAttributes = null) where T : class
        {
            var control = new Fields.LabelField<T>(container);
            control.DataControl.Text = value;
            control.DataControl.Name = value;
            control.Text = Text;
            control.HtmlAttributes = htmlAttributes;
            return (Fields.LabelField<T>)container.AddField(control);
        }
        public static Fields.FileboxField<T> AddFileboxField<T>(this FieldContainer<T> container, string ControlID, object htmlAttributes = null) where T : class
        {
            return AddFileboxField(container, ControlID, ControlID, htmlAttributes);
        }
        public static Fields.FileboxField<T> AddFileboxField<T>(this FieldContainer<T> container, string LabelText, string ControlID, object htmlAttributes = null) where T : class
        {
            var control = new Fields.FileboxField<T>(container);
            control.Text = LabelText;
            control.DataControl.ID = ControlID;
            control.DataControl.Name = ControlID;
            control.HtmlAttributes = htmlAttributes;
            return (Fields.FileboxField<T>)container.AddField(control);
        }
    }
}
