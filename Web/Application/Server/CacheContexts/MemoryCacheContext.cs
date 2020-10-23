using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Caching;
using System.Linq;

namespace Ophelia.Web.Application.Server.CacheContexts
{
    public class MemoryCacheContext: ICacheContext
    {
        private MemoryCache _MemoryCacheContext = MemoryCache.Default;
        public long CacheCount { get { return _MemoryCacheContext.GetCount(); } }
        public bool Add(string key, object value, DateTime absoluteExpiration)
        {
            bool result = false;
            try
            {
                if (_MemoryCacheContext.Contains(key))
                    _MemoryCacheContext.Remove(key);

                _MemoryCacheContext.Set(key, value, GetCachePolicy(key, absoluteExpiration));

                //Im-memory cache incnsistency. Added twice
                if (_MemoryCacheContext.Contains(key))
                    _MemoryCacheContext.Remove(key);

                _MemoryCacheContext.Set(key, value, GetCachePolicy(key, absoluteExpiration));
                result = true;
            }
            catch { return result; }
            return result;
        }

        private CacheItemPolicy GetCachePolicy(string key, DateTime absoluteExpiration)
        {
            if (absoluteExpiration <= DateTime.Now) absoluteExpiration = DateTime.Now.AddMinutes(CacheManager.CacheDuration);
            CacheItemPolicy cachingPolicy = new CacheItemPolicy
            {
                Priority = CacheItemPriority.Default,
                AbsoluteExpiration = absoluteExpiration,
                RemovedCallback = new CacheEntryRemovedCallback(CacheManager.OnCachedItemRemoved)
            };
            return cachingPolicy;
        }

        public bool Add(string keyGroup, string keyItem, object value, DateTime absoluteExpiration)
        {
            bool result = true;
            Dictionary<string, object> list = (Dictionary<string, object>)Get(keyGroup);
            if (list == null)
            {
                list = new Dictionary<string, object>();
                Add(keyGroup, list, absoluteExpiration);
            }
            object objectValue = null;
            if (list.TryGetValue(keyItem, out objectValue) && objectValue != null)
                list[keyItem] = value;
            else
                list.Add(keyItem, value);

            return result;
        }

        public bool ClearAll()
        {
            bool result = false;
            if (_MemoryCacheContext.GetCount() > 0)
            {
                result = true;
                foreach (var cache in _MemoryCacheContext)
                {
                    if (!Remove(cache.Key))
                        result = false;
                }
            }
            return result;
        }

        public bool Remove(string key)
        {
            bool result = false;
            try
            {
                string[] keys = key.Split(',');

                if (keys != null && keys.Length > 0)
                {
                    foreach (string sKey in keys)
                    {
                        if (_MemoryCacheContext.Contains(sKey))
                        {
                            _MemoryCacheContext.Remove(sKey);
                            result = true;
                        }
                    }
                }
            }
            catch { return result; }
            return result;
        }

        public object Get(string key)
        {
            return _MemoryCacheContext[key] as Object;
        }

        public T Get<T>(string key)
        {
            return (T)this.Get(key);
        }
        public object Get(string keyGroup, string keyItem)
        {
            object objectValue = null;
            Dictionary<string, object> list = (Dictionary<string, object>)Get(keyGroup);
            if (list != null)
                list.TryGetValue(keyItem, out objectValue);

            return objectValue;
        }

        public List<string> GetAllKeys()
        {
            return _MemoryCacheContext.Select(op => op.Key).ToList();
        }
    }
}
