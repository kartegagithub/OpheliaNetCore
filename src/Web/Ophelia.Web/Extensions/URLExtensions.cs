using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Ophelia;
using Ophelia.Net.Http;
using Ophelia.Service;
using Ophelia.Web.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Xml;

namespace Ophelia.Web
{
    public static partial class URLExtensions
    {
        public static Dictionary<string, IFormFile>? FilesToUpload { get; set; }
        public static List<FileData>? FilesToUploadBase64 { get; set; }

        public static TResult PostObject<T, TResult>(this string URL, T entity, dynamic parameters, WebHeaderCollection headers = null, bool PreAuthenticate = false, long languageID = 0)
        {
            var request = new WebApiObjectRequest<T>
            {
                Data = entity,
                LanguageID = languageID
            };
            SetParameters(request, parameters);
            return URL.GetObject<T, TResult>(request, headers, PreAuthenticate);
        }
        public static ServiceObjectResult<T> PostObject<T>(this string URL, T entity, WebHeaderCollection headers = null, bool PreAuthenticate = false, long languageID = 0)
        {
            var request = new WebApiObjectRequest<T>() { Data = entity, LanguageID = languageID };
            SetParameters(request, null);
            return URL.GetObject(request, headers, PreAuthenticate);
        }
        public static ServiceObjectResult<T> GetObject<T>(this string URL, long ID, dynamic parameters = null, WebHeaderCollection headers = null, bool PreAuthenticate = false)
        {
            var request = new WebApiObjectRequest<T>() { ID = ID };
            SetParameters(request, parameters);
            return URL.GetObject(request, headers, PreAuthenticate);
        }
        public static ServiceObjectResult<T> GetObjectByParam<T>(this string URL, dynamic parameters, WebHeaderCollection headers = null, bool PreAuthenticate = false)
        {
            var request = new WebApiObjectRequest<T>();
            SetParameters(request, parameters);
            return URL.GetObject(request, headers, PreAuthenticate);
        }
        public static TResult GetObject<T, TResult>(this string URL, WebApiObjectRequest<T> request, WebHeaderCollection headers = null, bool PreAuthenticate = false)
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
        public static ServiceObjectResult<T> GetObject<T>(this string URL, WebApiObjectRequest<T> request, WebHeaderCollection headers = null, bool PreAuthenticate = false)
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
            var request = new WebApiCollectionRequest<T>() { Page = page, PageSize = pageSize };
            SetParameters(request, parameters);
            return URL.GetCollection<T, ServiceCollectionResult<T>>(request, headers, PreAuthenticate);
        }
        public static TResult GetCollection<T, TResult>(this string URL, int page, int pageSize, dynamic parameters = null, WebHeaderCollection headers = null, bool PreAuthenticate = false)
        {
            var request = new WebApiCollectionRequest<T>() { Page = page, PageSize = pageSize };
            SetParameters(request, parameters);
            return URL.GetCollection<T, TResult>(request, headers, PreAuthenticate);
        }
        public static ServiceCollectionResult<T> GetCollection<T>(this string URL, int page, int pageSize, T filterEntity, dynamic parameters = null, WebHeaderCollection headers = null, bool PreAuthenticate = false)
        {
            var request = new WebApiCollectionRequest<T>() { Page = page, PageSize = pageSize, Data = filterEntity };
            SetParameters(request, parameters);
            return URL.GetCollection<T, ServiceCollectionResult<T>>(request, headers, PreAuthenticate);
        }
        public static TResult GetCollection<T, TResult>(this string URL, int page, int pageSize, T filterEntity, dynamic parameters = null, WebHeaderCollection headers = null, bool PreAuthenticate = false)
        {
            var request = new WebApiCollectionRequest<T>() { Page = page, PageSize = pageSize, Data = filterEntity };
            SetParameters(request, parameters);
            return URL.GetCollection<T, TResult>(request, headers, PreAuthenticate);
        }
        public static TResult GetCollection<T, TResult>(this string URL, WebApiCollectionRequest<T> request, WebHeaderCollection headers = null, bool PreAuthenticate = false)
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
        public static T PostURL<T, TEntity>(this string URL, WebApiObjectRequest<TEntity> request, WebHeaderCollection headers = null, bool PreAuthenticate = false)
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
                        serviceResult.Messages.Add(new ServiceResultMessage() { Code = "ERRAPI", Description = response });
                    }
                }
                return result;
            }
        }
        private static void SetParameters<T>(WebApiObjectRequest<T> request, dynamic parameters)
        {
            if (FilesToUpload != null)
            {
                foreach (var file in FilesToUpload.Where(op => op.Value != null && op.Value.Length > 0))
                {
                    request.Files.Add(file.Value.ToFileData(file.Key));
                }
            }
            FilesToUpload = null;
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
