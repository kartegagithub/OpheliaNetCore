using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Ophelia;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
namespace Ophelia.Web.View.Mvc.Html
{
    public static class EnumExtensions
    {
        public static HtmlString EnumListBox<TModel>(this IHtmlHelper<TModel> htmlHelper, string name, byte[] modelData, Type typeToDrawEnum, object htmlAttributes)
        {
            var source = Enum.GetValues(typeToDrawEnum);

            var displayAttributeType = typeof(DisplayAttribute);
            if (modelData == null)
                modelData = new byte[] { };
            string requestValue = Client.Current.Request.GetValue(name);
            if (string.IsNullOrEmpty(requestValue))
                requestValue = "";
            else
                requestValue = "," + requestValue + ",";

            var items = new List<SelectListItem>();
            foreach (var value in source)
            {
                FieldInfo field = value.GetType().GetField(value.ToString());

                var attrs = (DisplayAttribute)field.GetCustomAttributes(displayAttributeType, false).FirstOrDefault();
                object underlyingValue = Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()));
                var selected = modelData.Contains(Convert.ToByte(underlyingValue)) || requestValue.IndexOf("," + underlyingValue + ",") > -1;
                items.Add(new SelectListItem() { Selected = selected, Text = (attrs != null ? Translate(htmlHelper, attrs.GetName()) : Translate(htmlHelper, value.ToString())), Value = Convert.ToString(underlyingValue) });
            }
            var html = "<div class='enum_multi_select_box' id='" + name.Replace(".", "_") + "'>";
            var index = 0;
            foreach (var item in items)
            {
                html += "<div class='item'>";
                html += "<input type='checkbox' name='" + name + "' value='" + item.Value + "' id='" + name + "_" + index.ToString() + "' " + (item.Selected ? "checked" : "") + ">";
                html += htmlHelper.EnumDisplayFor(Convert.ToByte(item.Value), typeToDrawEnum, new { @for = name + "_" + index.ToString() }).ToString();
                html += "</div>";
                index++;
            }
            html += "</div>";

            return new HtmlString(html);
        }

        public static IHtmlContent EnumDropDownList<TModel>(this IHtmlHelper<TModel> htmlHelper, string name, object modelData, Type typeToDrawEnum, object htmlAttributes, string DefaultText = "", string DefaultValue = "")
        {
            var source = Enum.GetValues(typeToDrawEnum);

            var displayAttributeType = typeof(DisplayAttribute);

            var items = new List<SelectListItem>();
            foreach (var value in source)
            {
                FieldInfo field = value.GetType().GetField(value.ToString());

                var attrs = (DisplayAttribute)field.GetCustomAttributes(displayAttributeType, false).FirstOrDefault();
                object underlyingValue = Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()));

                items.Add(new SelectListItem() { Selected = underlyingValue.Equals(modelData), Text = (attrs != null ? Translate(htmlHelper, attrs.GetName()) : Translate(htmlHelper, value.ToString())), Value = Convert.ToString(underlyingValue) });
            }
            if (!string.IsNullOrEmpty(DefaultText))
                items.Insert(0, new SelectListItem() { Text = DefaultText, Value = DefaultValue });
            return htmlHelper.DropDownList(name, items, htmlAttributes);
        }

        public static IHtmlContent EnumDropDownListFor<TModel>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, short>> expression, Type typeToDrawEnum, object htmlAttributes = null, string DefaultText = "", string DefaultValue = "")
        {
            short model = 0;
            if (htmlHelper.ViewData.Model != null)
                model = expression.Compile().Invoke(htmlHelper.ViewData.Model);

            return htmlHelper.EnumDropDownList(ExpressionHelper.GetExpressionText(expression), model, typeToDrawEnum, htmlAttributes, DefaultText, DefaultValue);
        }

        public static IHtmlContent EnumDropDownListFor<TModel>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, byte>> expression, Type typeToDrawEnum, object htmlAttributes = null, string DefaultText = "", string DefaultValue = "")
        {
            byte model = 0;
            if (htmlHelper.ViewData.Model != null)
                model = expression.Compile().Invoke(htmlHelper.ViewData.Model);

            return htmlHelper.EnumDropDownList(ExpressionHelper.GetExpressionText(expression), model, typeToDrawEnum, htmlAttributes, DefaultText, DefaultValue);
        }

        public static IHtmlContent EnumDropDownListFor<TModel>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, byte?>> expression, Type typeToDrawEnum, object htmlAttributes = null, string DefaultText = "", string DefaultValue = "")
        {
            byte? model = 0;
            if (htmlHelper.ViewData.Model != null)
                model = expression.Compile().Invoke(htmlHelper.ViewData.Model);

            return htmlHelper.EnumDropDownList(ExpressionHelper.GetExpressionText(expression), model.GetValueOrDefault(0), typeToDrawEnum, htmlAttributes, DefaultText, DefaultValue);
        }

        public static HtmlString EnumListBoxFor<TModel>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, short>> expression, Type typeToDrawEnum, object htmlAttributes = null)
        {
            short model = 0;
            if (htmlHelper.ViewData.Model != null)
                model = expression.Compile().Invoke(htmlHelper.ViewData.Model);

            return htmlHelper.EnumListBox(ExpressionHelper.GetExpressionText(expression), model.ToString().ToByteList().ToArray(), typeToDrawEnum, htmlAttributes);
        }

        public static HtmlString EnumListBoxFor<TModel>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, byte>> expression, Type typeToDrawEnum, object htmlAttributes = null)
        {
            byte model = 0;
            if (htmlHelper.ViewData.Model != null)
                model = expression.Compile().Invoke(htmlHelper.ViewData.Model);

            return htmlHelper.EnumListBox(ExpressionHelper.GetExpressionText(expression), model.ToString().ToByteList().ToArray(), typeToDrawEnum, htmlAttributes);
        }

        public static HtmlString EnumListBoxFor<TModel>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, byte?>> expression, Type typeToDrawEnum, object htmlAttributes = null)
        {
            byte? model = 0;
            if (htmlHelper.ViewData.Model != null)
                model = expression.Compile().Invoke(htmlHelper.ViewData.Model);

            return htmlHelper.EnumListBox(ExpressionHelper.GetExpressionText(expression), model.GetValueOrDefault(0).ToString().ToByteList().ToArray(), typeToDrawEnum, htmlAttributes);
        }

        public static HtmlString EnumListBoxFor<TModel>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, string>> expression, Type typeToDrawEnum, object htmlAttributes = null)
        {
            string model = "";
            if (htmlHelper.ViewData.Model != null)
                model = expression.Compile().Invoke(htmlHelper.ViewData.Model);

            if (!string.IsNullOrEmpty(model))
                model = "";
            return htmlHelper.EnumListBox(ExpressionHelper.GetExpressionText(expression), model.ToByteList().ToArray(), typeToDrawEnum, htmlAttributes);
        }

        public static IHtmlContent EnumDisplayFor<TModel>(this IHtmlHelper<TModel> htmlHelper, object selectedValue, Type typeToDrawEnum, object htmlAttributes = null)
        {
            var source = Enum.GetValues(typeToDrawEnum);

            var displayAttributeType = typeof(DisplayAttribute);

            string selectedEnum = "";
            foreach (var value in source)
            {
                FieldInfo field = value.GetType().GetField(value.ToString());

                var attrs = (DisplayAttribute)field.GetCustomAttributes(displayAttributeType, false).FirstOrDefault();
                object underlyingValue = Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()));

                if (Convert.ToString(underlyingValue).Equals(Convert.ToString(selectedValue)))
                    selectedEnum = (attrs != null ? Translate(htmlHelper, attrs.GetName()) : Translate(htmlHelper, value.ToString()));
            }

            return htmlHelper.Label("", selectedEnum, htmlAttributes);
        }

        public static string Translate<TModel>(this IHtmlHelper<TModel> htmlHelper, string Key)
        {
            return Client.Current.TranslateText(Key);
        }
    }
}
