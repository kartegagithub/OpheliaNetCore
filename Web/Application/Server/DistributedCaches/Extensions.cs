using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ophelia.Web.Application.Server
{
    public static class Extensions
    {
        public static IServiceCollection AddRedisCache(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            services.AddTransient<IDistributedCache, DistributedCaches.RedisCache>();
            return services;
        }
    }
}
