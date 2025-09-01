using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ophelia.Caching.DistributedCaches
{
    public abstract class APICache : IDistributedCache, IDisposable
    {
        [ThreadStatic]
        protected static string _CacheKey;

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public virtual byte[] Get(string key)
        {
            return null;
        }

        public virtual Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            return null;
        }

        public virtual void Refresh(string key)
        {
            return;
        }

        public virtual Task RefreshAsync(string key, CancellationToken token = default)
        {
            return null;
        }

        public virtual void Remove(string key)
        {
            return;
        }

        public virtual Task RemoveAsync(string key, CancellationToken token = default)
        {
            return null;
        }

        public virtual void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            return;
        }

        public virtual Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            return null;
        }
    }
}
