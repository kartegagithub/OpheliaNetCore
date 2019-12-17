using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Ophelia
{
    /// <summary>
    /// 
    /// </summary>
    public static class IHtmlHelperExtensions
    {
        public static IDictionary<string, object> AnonymousObjectToHtmlAttributes(this IHtmlHelper htmlhelper, object htmlAttributes)
        {
            var dictionary = htmlAttributes as IDictionary<string, object>;
            if (dictionary != null)
            {
                return new Dictionary<string, object>(dictionary, StringComparer.OrdinalIgnoreCase);
            }

            dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            if (htmlAttributes != null)
            {
                var stringAttributes = htmlAttributes.ToString();
                stringAttributes = stringAttributes.Replace("{", "").Replace("}", "");
                string[] attributesArray = stringAttributes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var helper in attributesArray)
                {
                    string[] attribKeyValue = helper.Trim().Split(' ');
                    dictionary[attribKeyValue.First()] = attribKeyValue.Last();
                }
            }

            return dictionary;
        }
    }
}
