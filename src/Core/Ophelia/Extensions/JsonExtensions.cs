using Newtonsoft.Json;

namespace Ophelia
{
    public static class JsonExtensions
    {
        private static JsonSerializerSettings _toJsondefaultSettings;
        private static JsonSerializerSettings _fromJsondefaultSettings;
        public static void SetFromJsonDefaultSettings(JsonSerializerSettings settings)
        {
            _fromJsondefaultSettings = settings;
        }
        public static void SetToJsonDefaultSettings(JsonSerializerSettings settings)
        {
            _toJsondefaultSettings = settings;
        }
        public static string ToJson(this object obj, JsonSerializerSettings settings = null)
        {
            if (obj != null)
            {
                if (settings == null)
                {
                    if (_toJsondefaultSettings != null)
                    {
                        settings = _toJsondefaultSettings;
                    }
                    else
                    {
                        settings = new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                            MaxDepth = 3,
                            NullValueHandling = NullValueHandling.Ignore
                        };
                    }
                }
                return JsonConvert.SerializeObject(obj, Formatting.None, settings);
            }
            return string.Empty;
        }
        public static T FromJson<T>(this string value, JsonSerializerSettings settings = null)
        {
            if (settings == null)
            {
                if (_fromJsondefaultSettings != null)
                {
                    settings = _fromJsondefaultSettings;
                }
                else
                {
                    settings = new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        MaxDepth = 3,
                        NullValueHandling = NullValueHandling.Ignore
                    };
                }
            }
            return JsonConvert.DeserializeObject<T>(value, settings);
        }
    }
}
