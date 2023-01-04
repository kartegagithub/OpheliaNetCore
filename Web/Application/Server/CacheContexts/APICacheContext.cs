using System;
using System.Collections.Generic;
using Ophelia.Web.Application.Server.DistributedCaches;
using Newtonsoft.Json;

namespace Ophelia.Web.Application.Server.CacheContexts
{
    public class APICacheContext: ICacheContext
    {
        private APICache APICache { get; set; }

        public APICacheContext(APICache apiCache)
        {
            this.APICache = apiCache;
        }

        public long CacheCount { get { return 0; } }

        public bool Add(string key, object value, DateTime absoluteExpiration)
        {
            this.SetItem(key, value, absoluteExpiration);
            return true;
        }

        public bool Add(string keyGroup, string keyItem, object value, DateTime absoluteExpiration)
        {
            return false;
        }

        public bool ClearAll()
        {
            return true;
        }

        public object Get(string key)
        {
            return this.GetItem(key);
        }

        public T Get<T>(string key)
        {
            return this.GetItem<T>(key);
        }

        public object Get(string keyGroup, string keyItem)
        {
            return null;
        }

        public List<string> GetAllKeys()
        {
            return new List<string>();
        }

        public bool Remove(string key)
        {
            this.APICache.Remove(key);
            return true;
        }
        public bool Refresh(string key, DateTime absoluteExpiration)
        {
            this.APICache.Refresh(key);
            return true;
        }

        public void SetItem(string key, object value, Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions options = null)
        {
            this.APICache.Set(key, ToByteArray(value), options);
        }
        public void SetItem(string key, object value, DateTime expiration)
        {
            this.APICache.Set(key, ToByteArray(value), new Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions()
            {
                AbsoluteExpiration = expiration
            });
        }


        public object GetItem(string key)
        {
            var objResult = FromByteArray(this.APICache.Get(key));
            return objResult;
        }

        public T GetItem<T>(string key)
        {
            var objResult = FromByteArray<T>(this.APICache.Get(key));
            return objResult;
        }

        public byte[] ToByteArray(object obj)
        {
            if (obj == null)
                return null;

            return System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Ignore }));
        }
        public T FromByteArray<T>(byte[] data)
        {
            if (data == null)
                return default(T);

            return JsonConvert.DeserializeObject<T>(System.Text.Encoding.UTF8.GetString(data));
        }
        public object FromByteArray(byte[] data)
        {
            if (data == null)
                return default(object);

            return JsonConvert.DeserializeObject(System.Text.Encoding.UTF8.GetString(data));
        }
    }
}
