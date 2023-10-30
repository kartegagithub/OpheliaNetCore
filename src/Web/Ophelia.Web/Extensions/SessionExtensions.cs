using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Ophelia
{
    public static class SessionExtensions
    {
        public static void SetObject(this ISession session, string key, object value)
        {
            session.SetObject(key, value, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            });
        }

        public static void SetObject(this ISession session, string key, object value, JsonSerializerSettings settings)
        {
            session.SetString(key, JsonConvert.SerializeObject(value, settings));
        }

        public static T GetObject<T>(this ISession session, string key)
        {
            if (session == null)
                return default(T);

            var value = session.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }
}
