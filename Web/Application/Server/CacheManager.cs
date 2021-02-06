using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace Ophelia.Web.Application.Server
{
    public static class CacheManager
    {
        private static CacheContexts.ICacheContext _DefaultContext;
        private static Dictionary<string, CacheContexts.ICacheContext> _Contexts;
        private static Dictionary<string, CacheContexts.ICacheContext> Contexts
        {
            get
            {
                if (_Contexts == null)
                    _Contexts = new Dictionary<string, CacheContexts.ICacheContext>();
                return _Contexts;
            }
        }
        private static CacheContexts.ICacheContext DefaultContext
        {
            get
            {
                if (_DefaultContext == null)
                {
                    _DefaultContext = GetContext("DefaultMemoryCache");
                    if (_DefaultContext == null)
                        Register("DefaultMemoryCache", new CacheContexts.MemoryCacheContext(), true);
                }
                return _DefaultContext;
            }
        }
        public static CacheContexts.ICacheContext GetContext(string name)
        {
            if (Contexts.ContainsKey(name))
                return Contexts[name];
            return null;
        }
        public static CacheContexts.ICacheContext Register(string key, CacheContexts.ICacheContext context, bool useAsDefault = false)
        {
            if (Contexts.ContainsKey(key))
                return Contexts[key];

            Contexts.Add(key, context);
            if (useAsDefault)
                _DefaultContext = context;
            return context;
        }

        public static int CacheDuration
        {
            get { return ConfigurationManager.GetParameter<Int32>("CacheDuration", 1440); }
        }
        public static long CacheCount { get { return DefaultContext.CacheCount; } }

        public static bool AddByContext(string contextName, string key, object value, int duration = 0)
        {
            return AddByContext(contextName, key, value, DateTime.Now.AddMinutes(duration));
        }
        public static bool AddByContext(string contextName, string key, object value, DateTime absoluteExpiration)
        {
            return Contexts[contextName].Add(key, value, absoluteExpiration);
        }
        public static bool AddByContext(string contextName, string keyGroup, string keyItem, object value, int duration = 0)
        {
            return AddByContext(contextName, keyGroup, keyItem, value, DateTime.Now.AddMinutes(duration));
        }
        public static bool AddByContext(string contextName, string keyGroup, string keyItem, object value, DateTime absoluteExpiration)
        {
            return Contexts[contextName].Add(keyGroup, keyItem, value, absoluteExpiration);
        }

        public static bool Add(string key, object value, int duration = 0)
        {
            return Add(key, value, DateTime.Now.AddMinutes(duration));
        }
        public static bool Add(string key, object value, DateTime absoluteExpiration)
        {
            return DefaultContext.Add(key, value, absoluteExpiration);
        }
        public static bool Add(string keyGroup, string keyItem, object value, int duration = 0)
        {
            return Add(keyGroup, keyItem, value, DateTime.Now.AddMinutes(duration));
        }
        public static bool Add(string keyGroup, string keyItem, object value, DateTime absoluteExpiration)
        {
            return DefaultContext.Add(keyGroup, keyItem, value, absoluteExpiration);
        }
        public static bool ClearAll()
        {
            return DefaultContext.ClearAll();
        }
        public static bool ClearAllByContext(string contextName)
        {
            return Contexts[contextName].ClearAll();
        }
        public static bool RemoveByContext(string contextName, string key)
        {
            return Contexts[contextName].Remove(key);
        }

        public static bool Remove(string key)
        {
            return DefaultContext.Remove(key);
        }
        public static object GetByContext(string contextName, string key)
        {
            return Contexts[contextName].Get(key);
        }
        public static object GetByContext(string contextName, string keyGroup, string keyItem)
        {
            return Contexts[contextName].Get(keyGroup, keyItem);
        }
        public static object Get(string key)
        {
            return DefaultContext.Get(key);
        }
        public static T Get<T>(string key)
        {
            return DefaultContext.Get<T>(key);
        }
        public static object Get(string keyGroup, string keyItem)
        {
            return DefaultContext.Get(keyGroup, keyItem);
        }
        public static List<string> GetAllKeys()
        {
            return DefaultContext.GetAllKeys();
        }
        public static List<string> GetAllKeysByContext(string contextName)
        {
            return Contexts[contextName].GetAllKeys();
        }
        public static void OnCachedItemRemoved(CacheEntryRemovedArguments arguments)
        {
            string strLog = String.Concat("Reason: ", arguments.RemovedReason.ToString(), " | Key-Name: ", arguments.CacheItem.Key, " | Value-Object: ", arguments.CacheItem.Value.ToString());
        }
    }
}