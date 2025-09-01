using System;
using System.Collections.Generic;

namespace Ophelia.Caching
{
    public static class CacheManager
    {
        private static CacheContexts.ICacheContext _DefaultContext;
        private static Dictionary<string, CacheContexts.ICacheContext> _Contexts;
        private static object lockObj = new object();
        private static Dictionary<string, CacheContexts.ICacheContext> Contexts
        {
            get
            {
                _Contexts ??= new Dictionary<string, CacheContexts.ICacheContext>();
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
            if (Contexts.TryGetValue(name, out CacheContexts.ICacheContext value))
                return value;
            return null;
        }
        public static CacheContexts.ICacheContext Register(string key, CacheContexts.ICacheContext context, bool useAsDefault = false)
        {
            lock (lockObj)
            {
                if (Contexts.TryGetValue(key, out CacheContexts.ICacheContext value))
                    return value;

                Contexts.Add(key, context);
                if (useAsDefault)
                    _DefaultContext = context;
                return context;
            }
        }

        public static int CacheDuration
        {
            get { return 1440; }
        }
        public static long CacheCount { get { return DefaultContext.CacheCount; } }

        public static bool AddByContext(string contextName, string key, object value, int duration = 0)
        {
            return AddByContext(contextName, key, value, Utility.Now.AddMinutes(duration));
        }
        public static bool AddByContext(string contextName, string key, object value, DateTime absoluteExpiration)
        {
            return Contexts[contextName].Add(key, value, absoluteExpiration);
        }
        public static bool AddByContext(string contextName, string keyGroup, string keyItem, object value, int duration = 0)
        {
            return AddByContext(contextName, keyGroup, keyItem, value, Utility.Now.AddMinutes(duration));
        }
        public static bool AddByContext(string contextName, string keyGroup, string keyItem, object value, DateTime absoluteExpiration)
        {
            return Contexts[contextName].Add(keyGroup, keyItem, value, absoluteExpiration);
        }

        public static bool Add(string key, object value, int duration = 0)
        {
            return Add(key, value, Utility.Now.AddMinutes(duration));
        }
        public static bool Add(string key, object value, DateTime absoluteExpiration)
        {
            return DefaultContext.Add(key, value, absoluteExpiration);
        }
        public static bool Add(string keyGroup, string keyItem, object value, int duration = 0)
        {
            return Add(keyGroup, keyItem, value, Utility.Now.AddMinutes(duration));
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
        public static bool Refresh(string key, DateTime expiration)
        {
            return DefaultContext.Refresh(key, expiration);
        }
        public static bool RefreshByContext(string contextName, string key, DateTime expiration)
        {
            return Contexts[contextName].Refresh(key, expiration);
        }
        public static bool Remove(string key)
        {
            Console.WriteLine($"Cache_State:{key}::REMOVED");
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
        public static void Disconnect()
        {
            if (Contexts != null)
            {
                foreach (var context in Contexts)
                {
                    if (context.Value is IDisposable)
                    {
                        context.Value.Disconnect();
                    }
                }
            }
        }
    }
}