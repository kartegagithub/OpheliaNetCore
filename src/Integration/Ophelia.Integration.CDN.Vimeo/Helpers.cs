using Ophelia.Net.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Integration.CDN.Vimeo
{
    public static class Helpers
    {
        public static IWebProxy Proxy = null;

        public static byte[] ToByteArray(string s)
        {
            return Encoding.UTF8.GetBytes(s);
        }

        public static string ToBase64(string s)
        {
            return Convert.ToBase64String(ToByteArray(s));
        }

        public static string PercentEncode(string value)
        {
            const string unreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";
            var result = new StringBuilder();

            foreach (char symbol in value)
            {
                if (unreservedChars.IndexOf(symbol) != -1)
                    result.Append(symbol);
                else
                    result.Append('%' + String.Format("{0:X2}", (int)symbol));
            }

            return result.ToString();
        }

        public static async Task<string> HTTPFetchAsync(string url, string method,
            Dictionary<string, string> headers, Dictionary<string, string> payload,
            string contentType = "application/x-www-form-urlencoded")
        {
            return await HTTPFetchAsync(url, method, headers, payload.ToQueryString(), contentType);
        }

        public static async Task<string> HTTPFetchAsync(string url, string method, Dictionary<string, string> headers, string payload,
            string contentType = "application/x-www-form-urlencoded")
        {
            var factory = new RequestFactory()
                    .CreateClient(Proxy, null)
                    .AddAccept("application/vnd.vimeo.*+json; version=3.2")
                    .AddHeaders(headers)
                    .CreateRequest(url, method)
                    .CreateStringContent(payload, contentType);

            return await factory.GetStringResponseAsync();
        }

        public static HttpResponseMessage HTTPFetch(string url, string method, Dictionary<string, string> headers, Dictionary<string, string> payload, string contentType = "application/x-www-form-urlencoded")
        {
            return HTTPFetch(url, method, headers, payload.ToQueryString(), contentType);
        }

        public static HttpResponseMessage HTTPFetch(string url, string method, Dictionary<string, string> headers, string payload, string contentType = "application/x-www-form-urlencoded")
        {
            try
            {
                var factory = new RequestFactory()
                    .CreateClient(Proxy, null)
                    .AddAccept("application/vnd.vimeo.*+json; version=3.2")
                    .AddHeaders(headers)
                    .CreateRequest(url, method)
                    .CreateStringContent(payload, contentType);
                return factory.SendAsync().Result;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}