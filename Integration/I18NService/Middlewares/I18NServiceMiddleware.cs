using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ophelia.Integration.I18NService.Middlewares
{
    public class I18NServiceMiddleware
    {
        private readonly RequestDelegate _next;
        public I18NServiceMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (I18NIntegratorClient._Current != null)
                I18NIntegratorClient._Current.Flush();
            await _next(context);
        }
    }
}
