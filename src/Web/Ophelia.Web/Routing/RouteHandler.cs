using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
namespace Ophelia.Web.Routing
{
    public class RouteHandler : IRouter
    {
        public bool CheckSSL { get; set; }
        private RouteCollection oRoutes;
        private List<CustomRouteHandler> oCustomHandlers = new List<CustomRouteHandler>();
        protected virtual HttpStatusCode RedirectStatusCode { get; set; } = HttpStatusCode.TemporaryRedirect;
        public IRouter DefaulRouter { get; private set; }

        public List<CustomRouteHandler> CustomHandlers
        {
            get
            {
                if (this.oCustomHandlers == null)
                    this.oCustomHandlers = new List<CustomRouteHandler>();
                return this.oCustomHandlers;
            }
        }

        public RouteCollection Routes
        {
            get
            {
                return this.oRoutes;
            }
        }

        protected virtual bool CheckPattern(string friendlyUrl, RouteContext context)
        {
            var URL = (RouteItemURLPattern)this.Routes.GetPatternURL(friendlyUrl, this.GetLanguageCode());
            if (URL != null)
            {
                if (URL.RouteItem.IsSecure && context.HttpContext.Request.Scheme.Equals("http", StringComparison.InvariantCultureIgnoreCase) && context.HttpContext.Request.Host.ToString().IndexOf("localhost") == -1)
                {
                    context.HttpContext.Response.StatusCode = (int)this.RedirectStatusCode;
                    context.HttpContext.Response.AddHeader("Location", "https://" + context.HttpContext.Request.Host.ToString() + "/" + friendlyUrl);
                    return false;
                }
                else
                {
                    this.SetLanguageCode(URL.LanguageCode);
                    context.RouteData.Values["controller"] = URL.RouteItem.Controller;
                    context.RouteData.Values["action"] = URL.RouteItem.Action;
                    context.RouteData.Routers.Add(this);
                    URL.RouteItem.AddParamsToDictionary(friendlyUrl, context.RouteData.Values, URL);
                    if (URL.Pattern.Contains('{'))
                    {
                        var splitted = URL.Pattern.Split('/');
                        var splittedParts = friendlyUrl.Split('/');
                        for (int i = 0; i < splitted.Length; i++)
                        {
                            if (splitted[i].Contains('{'))
                            {
                                if (splittedParts.Length > i && !string.IsNullOrEmpty(splittedParts[i]))
                                {
                                    context.RouteData.Values[splitted[i].Replace("{", "").Replace("}", "")] = splittedParts[i];
                                }
                            }
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        protected virtual bool CheckFixedURL(string friendlyUrl, RouteContext context)
        {
            var URL = this.Routes.GetFixedURL(friendlyUrl, this.GetLanguageCode());
            if (URL != null)
            {
                if (this.CheckSSL && URL.RouteItem.IsSecure && context.HttpContext.Request.Scheme.Equals("http", StringComparison.InvariantCultureIgnoreCase) && !context.HttpContext.Request.Host.ToString().Contains("localhost"))
                {
                    context.HttpContext.Response.StatusCode = (int)this.RedirectStatusCode;
                    context.HttpContext.Response.AddHeader("Location", "https://" + context.HttpContext.Request.Host.ToString() + "/" + friendlyUrl);
                    context.HttpContext.Response.End();
                    return false;
                }
                else
                {
                    this.SetLanguageCode(URL.LanguageCode);
                    context.RouteData.Values["controller"] = URL.RouteItem.Controller;
                    context.RouteData.Values["action"] = URL.RouteItem.Action;
                    context.RouteData.Routers.Add(this);
                    if (!string.IsNullOrEmpty(URL.RouteItem.Area)) context.RouteData.Values["Area"] = URL.RouteItem.Area;
                    URL.RouteItem.AddParamsToDictionary(friendlyUrl, context.RouteData.Values);
                    return true;
                }
            }
            return false;
        }

        protected virtual void SetLanguageCode(string Code)
        {

        }
        protected virtual string GetLanguageCode()
        {
            return "en";
        }
        public CustomRouteHandler RegisterCustomHandler(CustomRouteHandler Handler)
        {
            if (Handler != null)
            {
                Handler.RouteHandler = this;
                this.CustomHandlers.Add(Handler);
            }
            return Handler;
        }

        public virtual async Task RouteAsync(RouteContext context)
        {
            bool found = false;
            try
            {
                this.OnBeforeRoute(context);

                string pageURL = Convert.ToString(context.RouteData.Values["pageURL"]);
                if (string.IsNullOrEmpty(Convert.ToString(pageURL)))
                    pageURL = context.HttpContext.Request.Path;

                if (pageURL != "/")
                {
                    var friendlyUrl = pageURL;
                    friendlyUrl = friendlyUrl.Trim('/');
                    bool HandledCustom = false;
                    foreach (var CustomHandler in this.CustomHandlers)
                    {
                        bool hasHandled = false;
                        var item = CustomHandler.Handle(context, friendlyUrl, out hasHandled);
                        if (!hasHandled && item != null)
                        {
                            if (!string.IsNullOrEmpty(item.Controller))
                            {
                                context.RouteData.Values["controller"] = item.Controller;
                                context.RouteData.Values["action"] = item.Action;
                                context.RouteData.Routers.Add(this);
                                if (!string.IsNullOrEmpty(item.Area)) context.RouteData.Values["area"] = item.Area;
                                item.AddParamsToDictionary(friendlyUrl, context.RouteData.Values);
                                HandledCustom = true;
                                found = true;
                            }
                            break;
                        }
                    }

                    if (!HandledCustom && this.Routes != null)
                    {
                        if (!this.CheckFixedURL(friendlyUrl.ToString(), context))
                        {
                            found = this.CheckPattern(friendlyUrl.ToString(), context);
                        }
                        else
                            found = true;
                    }
                }

                if (!found)
                    this.OnRouteNotFound(context, pageURL);
            }
            catch (Exception)
            {
                throw;
            }
            
            await this.DefaulRouter.RouteAsync(context);
        }
        protected virtual void OnRouteNotFound(RouteContext requestContext, string pageURL)
        {

        }
        public virtual VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            return this.DefaulRouter.GetVirtualPath(context);
        }
        protected virtual void OnBeforeRoute(RouteContext requestContext)
        {

        }
        public RouteHandler(IRouter defaulRouter, RouteCollection Routes, bool checkSSL)
        {
            this.DefaulRouter = defaulRouter;
            this.oRoutes = Routes;
            this.CheckSSL = checkSSL;
        }
    }
}
