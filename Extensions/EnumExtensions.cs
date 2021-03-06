﻿using Microsoft.AspNetCore.Mvc.Rendering;
using Ophelia.Web;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace Ophelia
{
    public static class EnumExtensions
    {
        public static Int32 ToInt32(this Enum value)
        {
            return Convert.ToInt32(value);
        }

        public static byte ToByte(this Enum value)
        {
            return Convert.ToByte(value);
        }

        public static string GetValue<T>(this Enum value)
        {
            return (Convert.ChangeType(value, typeof(T))).ToString();
        }
        public static List<TEnum> ToList<TEnum>()
        {
            Type type = typeof(TEnum);

            if (type.BaseType == typeof(Enum))
                throw new ArgumentException("T must be type of System.Enum");

            Array values = Enum.GetValues(type);
            if (values.Length > 0)
                return values.Cast<TEnum>().ToList();
            return null;
        }
        public static SelectListItem ToSelectListItem(this Enum value, bool isSelected = false)
        {
            return new SelectListItem { Text = value.ToString(), Value = value.GetValue<Int32>(), Selected = isSelected };
        }

        public static string ToClassName(this Enum style, string prefix)
        {
            var styleText = style.ToString()
                .ToDashCase()
                .ToLower(System.Globalization.CultureInfo.InvariantCulture);
            return string.Concat(prefix, styleText); ;
        }

        public static List<SelectListItem> GetEnumSelectList(this Type typeToDrawEnum, Client client)
        {
            var source = Enum.GetValues(typeToDrawEnum);

            var displayAttributeType = typeof(DisplayAttribute);

            var items = new List<SelectListItem>();
            foreach (var value in source)
            {
                FieldInfo field = value.GetType().GetField(value.ToString());

                if (!field.GetCustomAttributes(typeof(NotMappedAttribute), false).Any())
                {
                    var attrs = (DisplayAttribute)field.GetCustomAttributes(displayAttributeType, false).FirstOrDefault();
                    object underlyingValue = Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()));

                    items.Add(new SelectListItem() { Text = (attrs != null ? client.TranslateText(attrs.GetName()) : client.TranslateText(value.ToString())), Value = Convert.ToString(underlyingValue) });
                }
            }
            return items;
        }

        public static string GetEnumDisplayName(this Type typeToDrawEnum, object selectedValue, Client client)
        {
            var source = Enum.GetValues(typeToDrawEnum);

            var displayAttributeType = typeof(DisplayAttribute);

            var items = new List<SelectListItem>();
            foreach (var value in source)
            {
                FieldInfo field = value.GetType().GetField(value.ToString());

                var attrs = (DisplayAttribute)field.GetCustomAttributes(displayAttributeType, false).FirstOrDefault();
                object underlyingValue = Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()));
                if (Convert.ToString(underlyingValue) == Convert.ToString(selectedValue))
                {
                    return (attrs != null ? client.TranslateText(attrs.GetName()) : client.TranslateText(value.ToString()));
                }
            }
            return "";
        }
    }
}
