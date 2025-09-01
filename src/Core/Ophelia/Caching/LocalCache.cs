using System.Collections.Generic;

namespace Ophelia.Caching
{
    internal class LocalCache
    {
        private static Dictionary<string, object> _Cache = new Dictionary<string, object>();
        private static object lockObj = new object();
        public static object Get(string key)
        {
            lock (lockObj)
            {
                if (_Cache.TryGetValue(key, out object value))
                    return value;
                return null;
            }
        }
        public static void Update(string key, object value)
        {
            lock (lockObj)
            {
                if (!_Cache.ContainsKey(key))
                    _Cache.Add(key, value);
                else _Cache[key] = value;
            }
        }
        public static void Remove(string key)
        {
            lock (lockObj)
            {
                if (_Cache.ContainsKey(key))
                    _Cache.Remove(key);
            }
        }
    }
}
