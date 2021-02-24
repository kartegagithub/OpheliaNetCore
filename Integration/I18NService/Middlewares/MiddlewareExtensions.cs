using Ophelia;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Ophelia.Integration.I18NService.Middlewares
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseI18NService(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<I18NServiceMiddleware>();
            return builder;
        }

        public static void UseI18NService(this IServiceCollection services)
        {
            services.AddSingleton<I18NServiceMiddleware>();
        }
    }

}
