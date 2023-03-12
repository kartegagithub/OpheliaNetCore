using System.Net.Http;

namespace Ophelia
{
    public static class HttpClientExtensions
    {
        public static HttpMethod GetHttpMethod(string method)
        {
            method = method.ToUpperInvariant().ClearTurkishChars();
            switch (method)
            {
                case "POST":
                    return HttpMethod.Post;
                case "PUT":
                    return HttpMethod.Put;
                case "DELETE":
                    return HttpMethod.Delete;
                case "HEAD":
                    return HttpMethod.Head;
                case "OPTIONS":
                    return HttpMethod.Options;
                case "TRACE":
                    return HttpMethod.Trace;
                default:
                    return HttpMethod.Get;
            }
        }
    }
}
