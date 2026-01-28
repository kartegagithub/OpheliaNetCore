using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Ophelia;
using Ophelia.Service;
using System;
using System.Linq;
using System.Reflection;
using System.Web;

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

                                        var attribute = p.GetCustomAttributes(typeof(Ophelia.Data.Attributes.AllowHtml), checkBase: true).FirstOrDefault() as Ophelia.Data.Attributes.AllowHtml;
                                        if (attribute != null)
                                        {
                                            if (attribute.Sanitize)
                                                type = HtmlValidationProcessType.Sanitize;
                                            else if (attribute.Forbidden)
                                                type = HtmlValidationProcessType.RemoveHtml;
                                            else
                                                type = HtmlValidationProcessType.None;
                                        }
                                        p.SetValue(obj, this.ProcessHtml(strValue, attribute, type));
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
        protected virtual string ProcessHtml(string val, Ophelia.Data.Attributes.AllowHtml p, HtmlValidationProcessType type)
        {
            if (!string.IsNullOrEmpty(val))
                val = HttpUtility.HtmlDecode(val);
            switch (type)
            {
                case HtmlValidationProcessType.None:
                    return val;
                case HtmlValidationProcessType.Sanitize:
                    var Sanitizer = new Ganss.Xss.HtmlSanitizer();
                    Sanitizer.AllowedTags.Add("tel");
                    Sanitizer.AllowedTags.Add("iframe");
                    Sanitizer.AllowedTags.Add("meta");
                    Sanitizer.AllowedTags.Add("link");

                    Sanitizer.AllowedSchemes.Add("mailto");
                    Sanitizer.AllowedSchemes.Add("tel");

                    Sanitizer.AllowedAttributes.Add("content");
                    Sanitizer.AllowedAttributes.Add("property");
                    Sanitizer.AllowedAttributes.Add("sizes");
                    Sanitizer.AllowedAttributes.Add("rel");
                    Sanitizer.AllowedAttributes.Add("target");
                    Sanitizer.AllowedAttributes.Add("href");
                    Sanitizer.AllowedAttributes.Add("class");
                    Sanitizer.AllowedAttributes.Add("frameborder");
                    if (p != null)
                    {
                        if (p.AllowedTags != null)
                        {
                            var newTags = p.AllowedTags.Where(op => !Sanitizer.AllowedTags.Contains(op)).ToList();
                            Sanitizer.AllowedTags.AddRange(newTags);
                        }
                        if (p.AllowedSchemes != null)
                        {
                            var newTags = p.AllowedSchemes.Where(op => !Sanitizer.AllowedSchemes.Contains(op)).ToList();
                            Sanitizer.AllowedSchemes.AddRange(newTags);
                        }
                        if (p.AllowedAttributes != null)
                        {
                            var newTags = p.AllowedAttributes.Where(op => !Sanitizer.AllowedAttributes.Contains(op)).ToList();
                            Sanitizer.AllowedAttributes.AddRange(newTags);
                        }
                    }

                    return Sanitizer.Sanitize(val);
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