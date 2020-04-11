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
    public class RequestContentAntiXssMiddleware
    {
        private readonly RequestDelegate _next;
        public RequestContentAntiXssMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // Check XSS in request content
            var originalBody = context.Request.Body;
            try
            {
                var content = await ReadRequestBody(context);

                int matchIndex;
                if (CrossSiteScriptingValidation.IsDangerousString(content, out matchIndex))
                {
                    context.Response.Clear();
                    this.OnException(context);
                    return;
                }

                await _next(context);
            }
            finally
            {
                context.Request.Body = originalBody;
            }
        }

        protected void OnException(HttpContext context)
        {
            context.Response.Redirect("/ErrorPage?Type=XSS");
        }

        private static async Task<string> ReadRequestBody(HttpContext context)
        {
            var buffer = new MemoryStream();
            await context.Request.Body.CopyToAsync(buffer);
            context.Request.Body = buffer;
            buffer.Position = 0;

            var encoding = Encoding.UTF8;
            var requestContent = await new StreamReader(buffer, encoding).ReadToEndAsync();
            context.Request.Body.Position = 0;

            return requestContent;
        }
    }
}
