using Microsoft.AspNetCore.Http;

namespace Ophelia
{
    public static class HttpRequestExtension
    {
        public static string GetValue(this HttpRequest request, string key)
        {
            var val = "";
            if (request.Method == "POST" && request.Form.ContainsKey(key))
                val = request.Form[key];

            if (string.IsNullOrEmpty(val) && request.Query.ContainsKey(key))
                val = request.Query[key].ToString();
            return val;
        }

        public static string RawUrl(this HttpRequest request)
        {
            return Microsoft.AspNetCore.Http.Extensions.UriHelper.GetDisplayUrl(request);
        }
        public static string AbsolutePath(this HttpRequest request)
        {
            return string.Format("{0}://{1}{2}{3}", request.Scheme, request.Host, request.Path, request.QueryString);
        }
        public static bool IsLocalhost(this HttpRequest request)
        {
            if (request == null)
                return false;
            return request.Host.Host.IndexOf("localhost") > -1;
        }
    }
}
