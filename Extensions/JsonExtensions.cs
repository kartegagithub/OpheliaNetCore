using Newtonsoft.Json;

namespace Ophelia
{
    public static class JsonExtensions
    {
        public static string ToJson(this object obj, int recursionDepth)
        {
            if (obj != null)
                return JsonConvert.SerializeObject(obj, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, MaxDepth = recursionDepth });
            return string.Empty;
        }
        public static string ToJson(this object obj)
        {
            if (obj != null)
                return JsonConvert.SerializeObject(obj, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            return string.Empty;
        }
        public static T FromJson<T>(this string value)
        {
            return JsonConvert.DeserializeObject<T>(value, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }
    }
}
