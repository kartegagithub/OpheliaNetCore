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
        private Ganss.Xss.HtmlSanitizer Sanitizer;
        protected virtual string ProcessHtml(string val, Ophelia.Data.Attributes.AllowHtml p, HtmlValidationProcessType type)
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
                        this.Sanitizer = new Ganss.Xss.HtmlSanitizer();
                        if (p.AllowedTags != null)
                        {
                            var newTags = p.AllowedTags.Where(op => !this.Sanitizer.AllowedTags.Contains(op)).ToList();
                            this.Sanitizer.AllowedTags.AddRange(newTags);
                        }
                        if (p.AllowedSchemes != null)
                        {
                            var newTags = p.AllowedSchemes.Where(op => !this.Sanitizer.AllowedSchemes.Contains(op)).ToList();
                            this.Sanitizer.AllowedSchemes.AddRange(newTags);
                        }
                        if (p.AllowedAttributes != null)
                        {
                            var newTags = p.AllowedAttributes.Where(op => !this.Sanitizer.AllowedAttributes.Contains(op)).ToList();
                            this.Sanitizer.AllowedAttributes.AddRange(newTags);
                        }
                        this.Sanitizer.AllowedTags.Add("tel");
                        this.Sanitizer.AllowedTags.Add("iframe");
                        this.Sanitizer.AllowedTags.Add("meta");
                        this.Sanitizer.AllowedTags.Add("link");

                        this.Sanitizer.AllowedSchemes.Add("mailto");
                        this.Sanitizer.AllowedSchemes.Add("tel");

                        this.Sanitizer.AllowedAttributes.Add("content");
                        this.Sanitizer.AllowedAttributes.Add("property");
                        this.Sanitizer.AllowedAttributes.Add("sizes");
                        this.Sanitizer.AllowedAttributes.Add("rel");
                        this.Sanitizer.AllowedAttributes.Add("target");
                        this.Sanitizer.AllowedAttributes.Add("href");
                        this.Sanitizer.AllowedAttributes.Add("class");
                        this.Sanitizer.AllowedAttributes.Add("frameborder");
                    }
                    return this.Sanitizer.Sanitize(val);
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