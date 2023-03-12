using System;
using System.Linq;
using System.Web;
using Ophelia;
using Microsoft.AspNetCore.Mvc.Filters;
using Ophelia.Service;
using Microsoft.AspNetCore.Mvc.Controllers;
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

                                        var attribute = p.GetCustomAttributes(typeof(Ophelia.Data.Attributes.AllowHtml), checkBase: true).FirstOrDefault();
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
        private Ganss.Xss.HtmlSanitizer Sanitizer;
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
                        this.Sanitizer = new Ganss.Xss.HtmlSanitizer();

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