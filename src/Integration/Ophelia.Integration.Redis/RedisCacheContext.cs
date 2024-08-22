using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Ophelia;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Implementations;

namespace Ophelia.Integration.Redis
{
    public class RedisCacheContext : Ophelia.Caching.CacheContexts.ICacheContext
    {
        internal RedisCache _RedisCache { get; set; }
        public RedisCacheContext(IOptions<RedisCacheOptions> optionsAccessor)
        {
            _RedisCache = new RedisCache(optionsAccessor);
        }

        public long CacheCount { get { return ((RedisDatabase)_RedisCache.Database).SearchKeysAsync("*").Result.Count(); } }

        public bool Add(string key, object value, DateTime absoluteExpiration)
        {
            SetItem(key, value, absoluteExpiration);
            return true;
        }

        public bool Add(string keyGroup, string keyItem, object value, DateTime absoluteExpiration)
        {
            return false;
        }

        public bool ClearAll()
        {
            ((RedisDatabase)_RedisCache.Database).FlushDbAsync();
            return true;
        }

        public object Get(string key)
        {
            return GetItem(key);
        }

        public T Get<T>(string key)
        {
            return GetItem<T>(key);
        }

        public object Get(string keyGroup, string keyItem)
        {
            return null;
        }

        public List<string> GetAllKeys()
        {
            return ((RedisDatabase)_RedisCache.Database).SearchKeysAsync("*").Result.ToList();
        }

        public bool Remove(string key)
        {
            _RedisCache.Remove(key);
            return true;
        }
        public bool Refresh(string key, DateTime absoluteExpiration)
        {
            _RedisCache.Refresh(key);
            return true;
        }

        public void SetItem(string key, object value, Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions options = null)
        {
            _RedisCache.Set(key, ToByteArray(value), options);
        }
        public void SetItem(string key, object value, DateTime expiration)
        {
            _RedisCache.Set(key, ToByteArray(value), new Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions()
            {
                AbsoluteExpiration = expiration
            });
        }
        public object GetItem(string key)
        {
            var objResult = FromByteArray(_RedisCache.Get(key));
            return objResult;
        }

        public T GetItem<T>(string key)
        {
            var objResult = FromByteArray<T>(_RedisCache.Get(key));
            return objResult;
        }

        public byte[] ToByteArray(object obj)
        {
            if (obj == null)
                return null;

            return System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Ignore, ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
        }
        public T FromByteArray<T>(byte[] data)
        {
            if (data == null)
                return default;

            return JsonConvert.DeserializeObject<T>(System.Text.Encoding.UTF8.GetString(data));
        }
        public object FromByteArray(byte[] data)
        {
            if (data == null)
                return default;

            return JsonConvert.DeserializeObject(System.Text.Encoding.UTF8.GetString(data));
        }
    }
}
