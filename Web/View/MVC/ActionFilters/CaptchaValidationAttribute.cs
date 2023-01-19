using Microsoft.AspNetCore.Mvc.Filters;
using Ophelia.Web.View.Mvc.Html;
using Ophelia.Web.View.Mvc.Controllers.Base;

namespace Ophelia.Web.View.Mvc.ActionFilters
{
    public class CaptchaValidationAttribute : ActionFilterAttribute
    {
        public bool AlwaysShow { get; set; }
        public int ErrorCount { get; set; }
        public string EncKey { get; set; }
        public CaptchaValidationAttribute()
        {
            
        }
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            var request = actionContext.HttpContext.Request;
            var captcha = new Captcha();
            captcha.CaptchaModel.AlwaysShow = AlwaysShow;
            captcha.CaptchaModel.ErrorCount = ErrorCount;
            captcha.EncKey = this.EncKey;
            if (captcha.Enabled())
            {
                var InputCaptcha = request.Form["CaptchaCode"];
                var CaptchaSessionKey = request.Form["CKey"];
                if (!captcha.CheckCaptcha(InputCaptcha, CaptchaSessionKey))
                    ((Microsoft.AspNetCore.Mvc.Controller)actionContext.Controller).ModelState.AddModelError("CaptchaError", captcha.CaptchaModel.AlertMessage.Message);
            }
        }
    }
}
