using Microsoft.Extensions.Caching.Distributed;
using Ophelia.Web.Application.Server.CacheContexts;
using StackExchange.Redis.Extensions.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ophelia.Web.Application.Server.DistributedCaches
{
    public class RedisCache : IDistributedCache, IDisposable
    {
        [ThreadStatic]
        private static string _CacheKey;

        public static string CacheKey { get { return _CacheKey; } }

        public RedisCacheOptions Options { get; set; }
        public RedisCacheContext Context { get; private set; }
        private byte[] SessionData { get; set; }
        public RedisCache()
        {
            this.Options = new RedisCacheOptions();
        }
        public void Dispose()
        {
            
        }
        private IRedisDatabase GetDb()
        {
            if (this.Context == null)
                this.Context = (RedisCacheContext)CacheManager.GetContext(this.Options.ContextName);
            return this.Context.GetDb();
        }
        public byte[] Get(string key)
        {
            if (this.SessionData != null)
                return this.SessionData;

            _CacheKey = key;
            return this.GetDb().GetAsync<byte[]>(key).Result;
        }

        public Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            return this.GetDb().GetAsync<byte[]>(key);
        }

        public void Refresh(string key)
        {
            this.SessionData = null;
            //this.Remove(key);
            //this.Get(key);
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            this.SessionData = null;
            //this.RemoveAsync(key);
            return this.GetAsync(key);
        }

        public void Remove(string key)
        {
            var result = this.GetDb().RemoveAsync(key).Result;
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            return this.GetDb().RemoveAsync(key);
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            var result = this.GetDb().AddAsync<byte[]>(key, value).Result;
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            return this.GetDb().AddAsync<byte[]>(key, value);
        }
    }
}
