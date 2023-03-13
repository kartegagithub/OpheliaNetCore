using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Ophelia.Web.Routing
{
    public class RouteHandler : IRouter
    {
        public bool CheckSSL { get; set; }
        private RouteCollection oRoutes;
        private List<CustomRouteHandler> oCustomHandlers;
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
                    context.HttpContext.Response.StatusCode = 301;
                    context.HttpContext.Response.AddHeader("Location", "https://" + context.HttpContext.Request.Host.ToString() + "/" + friendlyUrl);
                    return false;
                }
                else
                {
                    this.SetLanguageCode(URL.LanguageCode);
                    context.RouteData.Values["controller"] = URL.RouteItem.Controller;
                    context.RouteData.Values["action"] = URL.RouteItem.Action;
                    URL.RouteItem.AddParamsToDictionary(friendlyUrl, context.RouteData.Values, URL);
                    if (URL.Pattern.IndexOf("{") > -1)
                    {
                        var splitted = URL.Pattern.Split('/');
                        var splittedParts = friendlyUrl.Split('/');
                        for (int i = 0; i < splitted.Length; i++)
                        {
                            if (splitted[i].Contains("{"))
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
                if (this.CheckSSL && URL.RouteItem.IsSecure && context.HttpContext.Request.Scheme.Equals("http", StringComparison.InvariantCultureIgnoreCase) && context.HttpContext.Request.Host.ToString().IndexOf("localhost") == -1)
                {
                    context.HttpContext.Response.StatusCode = 301;
                    context.HttpContext.Response.AddHeader("Location", "https://" + context.HttpContext.Request.Host.ToString() + "/" + friendlyUrl);
                    context.HttpContext.Response.End();
                    return false;
                }
                else
                {
                    this.SetLanguageCode(URL.LanguageCode);
                    context.RouteData.Values["controller"] = URL.RouteItem.Controller;
                    context.RouteData.Values["action"] = URL.RouteItem.Action;
                    context.RouteData.Values["Area"] = URL.RouteItem.Area;
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

        public virtual async Task RouteAsync(RouteContext requestContext)
        {
            bool found = false;
            try
            {
                this.OnBeforeRoute(requestContext);

                string pageURL = Convert.ToString(requestContext.RouteData.Values["pageURL"]);
                if (string.IsNullOrEmpty(Convert.ToString(pageURL)))
                    pageURL = requestContext.HttpContext.Request.Path;

                if (pageURL != "/")
                {
                    var friendlyUrl = pageURL;
                    friendlyUrl = friendlyUrl.Trim('/');
                    bool HandledCustom = false;
                    foreach (var CustomHandler in this.CustomHandlers)
                    {
                        bool hasHandled = false;
                        var item = CustomHandler.Handle(requestContext, friendlyUrl, out hasHandled);
                        if (!hasHandled && item != null)
                        {
                            if (!string.IsNullOrEmpty(item.Controller))
                            {
                                requestContext.RouteData.Values["controller"] = item.Controller;
                                requestContext.RouteData.Values["action"] = item.Action;
                                requestContext.RouteData.Values["area"] = item.Area;
                                item.AddParamsToDictionary(friendlyUrl, requestContext.RouteData.Values);
                                HandledCustom = true;
                                found = true;
                            }
                            break;
                        }
                    }

                    if (!HandledCustom && this.Routes != null)
                    {
                        if (!this.CheckFixedURL(friendlyUrl.ToString(), requestContext))
                        {
                            found = this.CheckPattern(friendlyUrl.ToString(), requestContext);
                        }
                        else
                            found = true;
                    }
                }

                if (!found)
                    this.OnRouteNotFound(requestContext, pageURL);
            }
            catch (Exception)
            {
                throw;
            }

            await this.DefaulRouter.RouteAsync(requestContext);
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
        public RouteHandler(IRouter defaulRouter, Web.Routing.RouteCollection Routes, bool checkSSL)
        {
            this.DefaulRouter = defaulRouter;
            this.oRoutes = Routes;
            this.CheckSSL = checkSSL;
        }
    }
}
