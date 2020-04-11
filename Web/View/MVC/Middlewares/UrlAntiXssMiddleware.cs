using Microsoft.AspNetCore.Http;
using Ophelia.Web.Application.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Web.View.Mvc.Middlewares
{
    public class UrlAntiXssMiddleware
    {
        private readonly RequestDelegate _next;
        public UrlAntiXssMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // Check XSS in URL
            if (!string.IsNullOrWhiteSpace(context.Request.Path.Value))
            {
                var url = context.Request.Path.Value;

                int matchIndex;
                if (CrossSiteScriptingValidation.IsDangerousString(url, out matchIndex))
                {
                    context.Response.Clear();
                    this.OnException(context);
                    return;
                }
            }

            // Check XSS in query string
            if (!string.IsNullOrWhiteSpace(context.Request.QueryString.Value))
            {
                var queryString = WebUtility.UrlDecode(context.Request.QueryString.Value);

                int matchIndex;
                if (CrossSiteScriptingValidation.IsDangerousString(queryString, out matchIndex))
                {
                    context.Response.Clear();
                    this.OnException(context);
                    return;
                }
            }
            await _next(context);
        }

        protected void OnException(HttpContext context)
        {
            context.Response.Redirect("/ErrorPage?Type=XSS");
        }
    }

    [Serializable]
    internal class CrossSiteScriptingException : Exception
    {
        public CrossSiteScriptingException()
        {
        }

        public CrossSiteScriptingException(string message) : base(message)
        {
        }

        public CrossSiteScriptingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CrossSiteScriptingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
