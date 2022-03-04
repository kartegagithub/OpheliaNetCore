using System;
using System.Linq;
using System.Web;
using Ophelia;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Ophelia.Service;
using Microsoft.AspNetCore.Mvc.Controllers;
using Ophelia.Cryptography;
using Ophelia.Configuration;
using System.Reflection;

namespace Ophelia.Web.View.Mvc.ActionFilters
{
    public class HtmlValidationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            var failResult = new ServiceObjectResult<bool>();
            try
            {
                var parameters = (actionContext.ActionDescriptor as ControllerActionDescriptor).MethodInfo.GetParameters();
                foreach (var item in parameters)
                {
                    if (!actionContext.ActionArguments.ContainsKey(item.Name))
                        continue;

                    var val = actionContext.ActionArguments[item.Name];
                    if (val != null && item.ParameterType.IsClass)
                    {
                        new Reflection.ObjectIterator()
                        {
                            IterationCallback = (obj) =>
                            {
                                var strProps = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(op => op.PropertyType.IsAssignableFrom(typeof(string))).ToList();
                                foreach (var p in strProps)
                                {
                                    if (p.SetMethod == null || !p.SetMethod.IsPublic || p.GetMethod == null || !p.GetMethod.IsPublic)
                                        continue;

                                    var strValue = p.GetValue(obj) as string;
                                    if (strValue != null && !string.IsNullOrEmpty(strValue))
                                    {
                                        HtmlValidationProcessType type = HtmlValidationProcessType.Sanitize;

                                        var attribute = p.GetCustomAttributes(typeof(Ophelia.Data.Attributes.AllowHtml)).FirstOrDefault();
                                        if (attribute != null)
                                        {
                                            if ((attribute as Ophelia.Data.Attributes.AllowHtml).Sanitize)
                                                type = HtmlValidationProcessType.Sanitize;
                                            else if ((attribute as Ophelia.Data.Attributes.AllowHtml).Forbidden)
                                                type = HtmlValidationProcessType.RemoveHtml;
                                            else
                                                type = HtmlValidationProcessType.None;
                                        }
                                        p.SetValue(obj, this.ProcessHtml(strValue, p, type));
                                    }
                                }
                                return null;
                            }
                        }.Iterate(val).Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                failResult.Fail("NotAuthorized", ex.Message);
            }
            base.OnActionExecuting(actionContext);
        }
        private Ganss.XSS.HtmlSanitizer Sanitizer;
        protected virtual string ProcessHtml(string val, PropertyInfo p, HtmlValidationProcessType type)
        {
            if (!string.IsNullOrEmpty(val))
                val = HttpUtility.HtmlDecode(val);
            switch (type)
            {
                case HtmlValidationProcessType.None:
                    return val;
                case HtmlValidationProcessType.Sanitize:
                    if (this.Sanitizer == null)
                    {
                        var allowedTags = Ganss.XSS.HtmlSanitizer.DefaultAllowedTags;
                        allowedTags.Add("iframe");
                        allowedTags.Add("meta");
                        allowedTags.Add("link");

                        var allowedAttributes = Ganss.XSS.HtmlSanitizer.DefaultAllowedAttributes;
                        allowedAttributes.Add("content");
                        allowedAttributes.Add("property");
                        allowedAttributes.Add("sizes");
                        allowedAttributes.Add("rel");
                        allowedAttributes.Add("target");
                        allowedAttributes.Add("href");
                        allowedAttributes.Add("class");
                        allowedAttributes.Add("frameborder");

                        var allowedSchemes = Ganss.XSS.HtmlSanitizer.DefaultAllowedSchemes;
                        allowedSchemes.Add("mailto");
                        allowedSchemes.Add("tel");

                        this.Sanitizer = new Ganss.XSS.HtmlSanitizer(allowedTags, allowedSchemes, allowedAttributes);
                    }
                    return this.Sanitizer.Sanitize(val).CheckHTMLOnFuntions();
                case HtmlValidationProcessType.RemoveHtml:
                    return val.RemoveHTML().CheckHTMLOnFuntions();
            }
            return val;
        }
    }

    public enum HtmlValidationProcessType
    {
        None,
        Sanitize,
        RemoveHtml,
    }
}