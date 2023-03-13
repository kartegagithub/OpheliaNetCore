﻿using Newtonsoft.Json;
using Ophelia;
using Ophelia.Net.Http;
using Ophelia.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Xml;

namespace Ophelia
{
    public static partial class URLExtensions
    {
        public static int Timeout { get; set; }
        public static Func<HttpClientHandler, DelegatingHandler> RequestLogHandler { get; set; }
        public static Action<HttpResponseMessage> OnResponseHandler { get; set; }

        public static T PostURL<T>(this string URL, dynamic parameters, WebHeaderCollection headers = null, bool PreAuthenticate = false, string contentType = "application/x-www-form-urlencoded", NetworkCredential credential = null)
        {
            var sParams = "";
            if (parameters != null)
            {
                var jsonParams = Newtonsoft.Json.JsonConvert.DeserializeObject<IDictionary<string, object>>(Newtonsoft.Json.JsonConvert.SerializeObject(parameters));
                foreach (var item in jsonParams.Keys)
                {
                    if (!string.IsNullOrEmpty(sParams))
                        sParams += "&";

                    sParams += item + "=" + JsonConvert.SerializeObject(jsonParams[item]);
                }
            }
            var response = URL.PostURL(sParams, contentType, headers, PreAuthenticate, credential);
            if (!string.IsNullOrEmpty(response))
            {
                if (response.StartsWith("<", StringComparison.InvariantCultureIgnoreCase))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(response);
                    response = JsonConvert.SerializeXmlNode(doc);
                }
            }
            try
            {
                return JsonConvert.DeserializeObject<T>(response);
            }
            catch (Exception ex)
            {
                var result = (T)Activator.CreateInstance(typeof(T));
                if (result is ServiceResult)
                {
                    (result as ServiceResult).Fail(ex, "ERRAPI");
                    (result as ServiceResult).Messages.Add(new ServiceResultMessage() { Code = "ERRAPI", Description = response });
                }
                return result;
            }
        }
        public static T PostURL<T>(this string URL, string parameters, string contentType = "application/x-www-form-urlencoded", WebHeaderCollection headers = null, bool PreAuthenticate = false, NetworkCredential credential = null)
        {
            var response = URL.DownloadURL("POST", parameters, contentType, headers, PreAuthenticate, 120000, credential);
            if (!string.IsNullOrEmpty(response))
            {
                if (response.StartsWith("<", StringComparison.InvariantCultureIgnoreCase))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(response);
                    response = JsonConvert.SerializeXmlNode(doc);
                }
            }
            try
            {
                return JsonConvert.DeserializeObject<T>(response);
            }
            catch (Exception ex)
            {
                var result = (T)Activator.CreateInstance(typeof(T));
                if (result is ServiceResult)
                {
                    (result as ServiceResult).Fail(ex, "ERRAPI");
                    (result as ServiceResult).Messages.Add(new ServiceResultMessage() { Code = "ERRAPI", Description = response });
                }
                return result;
            }
        }
        public static string PostURL(this string URL, string parameters, string contentType = "application/x-www-form-urlencoded", WebHeaderCollection headers = null, bool PreAuthenticate = false, NetworkCredential credential = null)
        {
            return URL.DownloadURL("POST", parameters, contentType, headers, PreAuthenticate, 120000, credential);
        }
        public static string DownloadURL(this string url, string method = "GET", string parameters = "", string contentType = "application/x-www-form-urlencoded", WebHeaderCollection headers = null, bool preAuthenticate = false, int timeout = 120000, NetworkCredential credential = null)
        {
            if (!string.IsNullOrEmpty(parameters))
            {
                if (method == "GET")
                {
                    if (!url.Contains('?'))
                        url += "?";
                    url += parameters;
                }
            }

            var factory = new RequestFactory()
                .SetLogHandler(URLExtensions.RequestLogHandler)
                .SetOnResponse(URLExtensions.OnResponseHandler)
                .CreateClient()
                .SetTimeout(timeout)
                .SetCredentials(credential)
                .SetPreAuthenticate(preAuthenticate)
                .AddHeaders(headers)
                .CreateRequest(url, method)
                .CreateStringContent(parameters, contentType);
            return factory.GetStringResponse();
        }
        
        public static PingReply Ping(string address, string data = "test")
        {
            var pingSender = new Ping();
            var options = new PingOptions()
            {
                DontFragment = true
            };

            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 120;
            return pingSender.Send(address, timeout, buffer, options);
        }

        public static string CombineURL(this string baseURI, params string[] segments)
        {
            if (segments != null && segments.Any())
            {
                var uri = baseURI;
                foreach (var item in segments)
                {
                    uri = $"{uri}/{item.Trim('/')}";
                }
                return uri;
            }
            return baseURI;
        }
    }
}
