using Microsoft.AspNetCore.Http;
using System.Text;
using System.Linq;

namespace Ophelia
{
    public static class HttpRequestExtension
    {
        public static string GetValue(this HttpRequest request, string key)
        {
            if (request == null)
                return "";

            var val = "";
            if (request.Method == "POST" && request.Form.ContainsKey(key))
                val = request.Form[key];

            if (string.IsNullOrEmpty(val) && request.Query.ContainsKey(key))
                val = request.Query[key].ToString();
            return val;
        }
        public static string RawUrl(this HttpRequest request)
        {
            var sb = new StringBuilder();
            sb.Append(request.PathBase.Value).Append(request.Path.Value);
            if (request.Query.Any())
                sb.Append("?");
            foreach (var item in request.Query)
            {
                sb.Append(item.Key).Append("=");
                if (!string.IsNullOrEmpty(item.Value))
                {
                    sb.Append(item.Value.ToString().RemoveHTML().CheckHTMLOnFuntions().EncodeJavascript());
                }
                if (item.Key != request.Query.LastOrDefault().Key)
                    sb.Append("&");
            }
            return sb.ToString();
        }
        public static string AbsolutePath(this HttpRequest request)
        {
            return string.Format("{0}://{1}{2}", request.Scheme, request.Host, request.RawUrl());
        }
        public static string BasePath(this HttpRequest request)
        {
            return string.Format("{0}://{1}/", request.Scheme, request.Host);
        }
        public static bool IsLocalhost(this HttpRequest request)
        {
            if (request == null)
                return false;
            return request.Host.Host.IndexOf("localhost") > -1 || request.Host.Host == "127.0.0.1" || request.Host.Host == "::1";
        }
    }
}
