using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Ophelia.Caching.CacheContexts
{
    public class MemoryCacheContext : ICacheContext, IDisposable
    {
        private MemoryCache _MemoryCacheContext = new MemoryCache(new MemoryCacheOptions() { });

        public long CacheCount { get { return _MemoryCacheContext.Count; } }
        public bool Add(string key, object value, DateTime absoluteExpiration)
        {
            bool result = false;
            try
            {
                try
                {
                    _MemoryCacheContext.Remove(key);
                }
                catch (Exception)
                {

                }
                _MemoryCacheContext.Set(key, value, GetCachePolicy(absoluteExpiration));
                result = true;
            }
            catch { return result; }
            return result;
        }

        private MemoryCacheEntryOptions GetCachePolicy(DateTime absoluteExpiration)
        {
            if (absoluteExpiration <= Utility.Now) absoluteExpiration = Utility.Now.AddMinutes(CacheManager.CacheDuration);
            var cachingPolicy = new MemoryCacheEntryOptions
            {
                Priority = CacheItemPriority.Normal,
                AbsoluteExpiration = absoluteExpiration
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
            object objectValue;
            if (list.TryGetValue(keyItem, out objectValue) && objectValue != null)
                list[keyItem] = value;
            else
                list.Add(keyItem, value);

            return result;
        }

        public bool ClearAll()
        {
            bool result = false;
            if (this.CacheCount > 0)
            {
                _MemoryCacheContext.Clear();
                result = true;
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
                        try
                        {
                            _MemoryCacheContext.Remove(sKey);
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }
            catch { return result; }
            return result;
        }

        public object Get(string key)
        {
            return _MemoryCacheContext.Get(key);
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
            try
            {
                var field = typeof(MemoryCache).GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);
                var collection = field.GetValue(_MemoryCacheContext) as ICollection;
                var items = new List<string>();
                if (collection != null)
                {
                    foreach (var item in collection)
                    {
                        var methodInfo = item.GetType().GetProperty("Key");
                        var val = methodInfo.GetValue(item);
                        items.Add(val.ToString());
                    }
                }
                return items;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            return new List<string>();
        }

        public bool Refresh(string key, DateTime absoluteExpiration)
        {
            var val = this.Get(key);
            this.Remove(key);
            if (val != null)
            {
                this.Add(key, val, absoluteExpiration);
                return true;
            }
            return false;
        }

        public void Disconnect()
        {

        }

        public void Dispose()
        {
            this.Disconnect();
            GC.SuppressFinalize(this);
        }
    }
}
