using Newtonsoft.Json;
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
        public static int Timeout { get; set; } = 120000;
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
            var response = URL.DownloadURL("POST", parameters, contentType, headers, PreAuthenticate, URLExtensions.Timeout, credential);
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
            return URL.DownloadURL("POST", parameters, contentType, headers, PreAuthenticate, URLExtensions.Timeout, credential);
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
                .SetLogHandler(RequestLogHandler)
                .SetOnResponse(OnResponseHandler)
                .CreateClient()
                .SetTimeout(timeout)
                .SetCredentials(credential)
                .SetPreAuthenticate(preAuthenticate)
                .AddHeaders(headers)
                .CreateRequest(url, method)
                .CreateStringContent(parameters, contentType);
            return factory.GetStringResponse();
        }

        public static List<FileData>? FilesToUploadBase64 { get; set; }

        public static TResult PostObject<T, TResult>(this string URL, T entity, dynamic parameters, WebHeaderCollection headers = null, bool PreAuthenticate = false, long languageID = 0)
        {
            var request = new ServiceObjectRequest<T>
            {
                Data = entity,
                LanguageID = languageID
            };
            SetParameters(request, parameters);
            return URL.GetObject<T, TResult>(request, headers, PreAuthenticate);
        }
        public static ServiceObjectResult<T> PostObject<T>(this string URL, T entity, WebHeaderCollection headers = null, bool PreAuthenticate = false, long languageID = 0)
        {
            var request = new ServiceObjectRequest<T>() { Data = entity, LanguageID = languageID };
            SetParameters(request, null);
            return URL.GetObject(request, headers, PreAuthenticate);
        }
        public static ServiceObjectResult<T> GetObject<T>(this string URL, long ID, dynamic parameters = null, WebHeaderCollection headers = null, bool PreAuthenticate = false)
        {
            var request = new ServiceObjectRequest<T>() { ID = ID };
            SetParameters(request, parameters);
            return URL.GetObject(request, headers, PreAuthenticate);
        }
        public static ServiceObjectResult<T> GetObjectByParam<T>(this string URL, dynamic parameters, WebHeaderCollection headers = null, bool PreAuthenticate = false)
        {
            var request = new ServiceObjectRequest<T>();
            SetParameters(request, parameters);
            return URL.GetObject(request, headers, PreAuthenticate);
        }
        public static TResult GetObject<T, TResult>(this string URL, ServiceObjectRequest<T> request, WebHeaderCollection headers = null, bool PreAuthenticate = false)
        {
            var response = "";
            try
            {
                response = URL.PostURL(request.ToJson(), "application/json", headers, PreAuthenticate);
                return JsonConvert.DeserializeObject<TResult>(response);
            }
            catch (Exception ex)
            {
                var result = (TResult)Activator.CreateInstance(typeof(TResult));
                if (result is ServiceResult)
                {
                    if (result is ServiceResult serviceResult)
                    {
                        serviceResult.Fail(ex, "ERRAPI");
                        serviceResult.Messages.Add(new ServiceResultMessage() { Code = "ERRAPI", Description = response });
                    }
                }
                return result;
            }
        }
        public static ServiceObjectResult<T> GetObject<T>(this string URL, ServiceObjectRequest<T> request, WebHeaderCollection headers = null, bool PreAuthenticate = false)
        {
            var response = "";
            try
            {
                response = URL.PostURL(request.ToJson(), "application/json", headers, PreAuthenticate);
                return JsonConvert.DeserializeObject<ServiceObjectResult<T>>(response);
            }
            catch (Exception ex)
            {
                var result = (ServiceObjectResult<T>)Activator.CreateInstance(typeof(ServiceObjectResult<T>));
                if (result is ServiceResult serviceResult)
                {
                    serviceResult.Fail(ex, "ERRAPI");
                    serviceResult.Messages.Add(new ServiceResultMessage() { Code = "ERRAPI", Description = response });
                }
                return result;
            }
        }
        public static ServiceCollectionResult<T> GetCollection<T>(this string URL, int page, int pageSize, dynamic parameters = null, WebHeaderCollection headers = null, bool PreAuthenticate = false)
        {
            var request = new ServiceCollectionRequest<T>() { Page = page, PageSize = pageSize };
            SetParameters(request, parameters);
            return URL.GetCollection<T, ServiceCollectionResult<T>>(request, headers, PreAuthenticate);
        }
        public static TResult GetCollection<T, TResult>(this string URL, int page, int pageSize, dynamic parameters = null, WebHeaderCollection headers = null, bool PreAuthenticate = false)
        {
            var request = new ServiceCollectionRequest<T>() { Page = page, PageSize = pageSize };
            SetParameters(request, parameters);
            return URL.GetCollection<T, TResult>(request, headers, PreAuthenticate);
        }
        public static ServiceCollectionResult<T> GetCollection<T>(this string URL, int page, int pageSize, T filterEntity, dynamic parameters = null, WebHeaderCollection headers = null, bool PreAuthenticate = false)
        {
            var request = new ServiceCollectionRequest<T>() { Page = page, PageSize = pageSize, Data = filterEntity };
            SetParameters(request, parameters);
            return URL.GetCollection<T, ServiceCollectionResult<T>>(request, headers, PreAuthenticate);
        }
        public static TResult GetCollection<T, TResult>(this string URL, int page, int pageSize, T filterEntity, dynamic parameters = null, WebHeaderCollection headers = null, bool PreAuthenticate = false)
        {
            var request = new ServiceCollectionRequest<T>() { Page = page, PageSize = pageSize, Data = filterEntity };
            SetParameters(request, parameters);
            return URL.GetCollection<T, TResult>(request, headers, PreAuthenticate);
        }
        public static TResult GetCollection<T, TResult>(this string URL, ServiceCollectionRequest<T> request, WebHeaderCollection headers = null, bool PreAuthenticate = false)
        {
            var response = "";
            try
            {
                response = URL.PostURL(request.ToJson(), "application/json", headers, PreAuthenticate);
                return JsonConvert.DeserializeObject<TResult>(response);
            }
            catch (Exception ex)
            {
                var result = (TResult)Activator.CreateInstance(typeof(TResult));
                if (result is ServiceResult)
                {
                    if (result is ServiceResult serviceResult)
                    {
                        serviceResult.Fail(ex, "ERRAPI");
                        serviceResult.Messages.Add(new ServiceResultMessage() { Code = "ERRAPI", Description = response });
                    }
                }
                return result;
            }
        }
        public static T PostURL<T, TEntity>(this string URL, ServiceObjectRequest<TEntity> request, WebHeaderCollection headers = null, bool PreAuthenticate = false)
        {
            var response = "";
            try
            {
                response = URL.PostURL(request.ToJson(), "application/json", headers, PreAuthenticate);
                return JsonConvert.DeserializeObject<T>(response);
            }
            catch (Exception ex)
            {
                var result = (T)Activator.CreateInstance(typeof(T));
                if (result is ServiceResult)
                {
                    if (result is ServiceResult serviceResult)
                    {
                        serviceResult.Fail(ex, "ERRAPI");
                        if (string.IsNullOrEmpty(response)) response = ex.ToString();
                        serviceResult.Messages.Add(new ServiceResultMessage() { Code = "ERRAPI", Description = response });
                    }
                }
                return result;
            }
        }
        private static void SetParameters<T>(ServiceObjectRequest<T> request, dynamic parameters)
        {
            if (FilesToUploadBase64?.Count > 0)
            {
                foreach (var file in FilesToUploadBase64)
                {
                    request.Files.Add(file);
                }
            }
            FilesToUploadBase64 = null;
            if (parameters != null)
            {
                var jsonParams = Newtonsoft.Json.JsonConvert.DeserializeObject<IDictionary<string, string>>(Newtonsoft.Json.JsonConvert.SerializeObject(parameters));
                foreach (var item in jsonParams.Keys)
                {
                    request.Parameters[item] = Convert.ToString(jsonParams[item]);
                }
            }
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
