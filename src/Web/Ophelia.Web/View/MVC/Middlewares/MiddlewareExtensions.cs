using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Ophelia.Web.View.Mvc.Middlewares
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseUrlAntiXss(this IApplicationBuilder builder)
        {
            builder.UseMiddleware(typeof(UrlAntiXssMiddleware));
            return builder;
        }
        public static IApplicationBuilder UseRequestContentAntiXss(this IApplicationBuilder builder)
        {
            builder.UseMiddleware(typeof(RequestContentAntiXssMiddleware));
            return builder;
        }
        public static IServiceCollection AddHTTPContextAccessor(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            services.TryAddSingleton<IHttpContextAccessor, HTTPContextAccessor>();
            return services;
        }
        public static IServiceCollection AddHTTPContextAccessor<TAccessor>(this IServiceCollection services) where TAccessor : class, IHttpContextAccessor
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            services.TryAddSingleton<IHttpContextAccessor, TAccessor>();
            return services;
        }
    }
}
