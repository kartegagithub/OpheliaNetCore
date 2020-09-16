using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Net;
using System.Diagnostics;
using System.IO;

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

        public static string KeyValueToString(Dictionary<string, string> payload)
        {
            string body = "";
            foreach (var item in payload)
                body += String.Format("{0}={1}&",
                    Helpers.PercentEncode(item.Key),
                    Helpers.PercentEncode(item.Value));
            if (body[body.Length - 1] == '&') body = body.Substring(0, body.Length - 1);
            return body;
        }

        public static async Task<string> HTTPFetchAsync(string url, string method,
            WebHeaderCollection headers, Dictionary<string, string> payload,
            string contentType = "application/x-www-form-urlencoded")
        {
            return await HTTPFetchAsync(url, method, headers, KeyValueToString(payload), contentType);
        }

        public static async Task<string> HTTPFetchAsync(string url, string method, WebHeaderCollection headers, string payload,
            string contentType = "application/x-www-form-urlencoded")
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.CreateHttp(url);
            if (Proxy != null) request.Proxy = Proxy;

            request.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            request.Headers = headers;
            request.Method = method;
            request.Accept = "application/vnd.vimeo.*+json; version=3.2";
            request.ContentType = contentType;
            request.KeepAlive = false;

            var streamBytes = Helpers.ToByteArray(payload);
            request.ContentLength = streamBytes.Length;
            Stream dataStream = await request.GetRequestStreamAsync();
            await dataStream.WriteAsync(streamBytes, 0, streamBytes.Length);
            dataStream.Close();

            HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync());
            Debug.WriteLine(((HttpWebResponse)response).StatusDescription);

            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            reader.Close();
            dataStream.Close();

            response.Close();

            Debug.WriteLine(String.Format("Response from URL {0}:", url), "HTTPFetch");
            Debug.WriteLine(responseFromServer, "HTTPFetch");
            return responseFromServer;
        }

        public static HttpWebResponse HTTPFetch(string url, string method, WebHeaderCollection headers, Dictionary<string, string> payload, string contentType = "application/x-www-form-urlencoded")
        {
            return HTTPFetch(url, method, headers, KeyValueToString(payload), contentType);
        }

        public static HttpWebResponse HTTPFetch(string url, string method, WebHeaderCollection headers, string payload, string contentType = "application/x-www-form-urlencoded")
        {
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                       | SecurityProtocolType.Tls11
                       | SecurityProtocolType.Tls12
                       | SecurityProtocolType.Ssl3;

                HttpWebRequest request = (HttpWebRequest)WebRequest.CreateHttp(url);
                if (Proxy != null) request.Proxy = Proxy;

                if(headers != null)
                    request.Headers = headers;
                request.Method = method;
                request.Accept = "application/vnd.vimeo.*+json; version=3.2";
                request.ContentType = contentType;
                request.KeepAlive = false;
                request.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                if (!String.IsNullOrWhiteSpace(payload))
                {
                    var streamBytes = Helpers.ToByteArray(payload);
                    request.ContentLength = streamBytes.Length;
                    Stream reqStream = request.GetRequestStream();
                    reqStream.Write(streamBytes, 0, streamBytes.Length);
                    reqStream.Close();
                }
                else
                    request.ContentLength = 0;

                return request.GetResponseWithoutException();
            }
            catch (Exception ex)
            {

                throw;
            }            
        }
    }
}