using System;
using System.Collections.Generic;
using System.Text;

namespace Ophelia.Web.Application.Server.CacheContexts
{
    public interface ICacheContext
    {
        long CacheCount { get; }

        bool Add(string key, object value, DateTime absoluteExpiration);

        bool Add(string keyGroup, string keyItem, object value, DateTime absoluteExpiration);

        bool ClearAll();

        bool Remove(string key);

        object Get(string key);
        T Get<T>(string key);

        object Get(string keyGroup, string keyItem);

        List<string> GetAllKeys();
    }
}
