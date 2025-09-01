using Microsoft.AspNetCore.Http;
using Ophelia;
using Ophelia.Service;
using Ophelia.Web.Service;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Ophelia.Web
{
    public static partial class URLExtensions
    {
        public static Dictionary<string, IFormFile>? FilesToUpload { get; set; }
        public static TResult PostObject<T, TResult>(this string URL, T entity, dynamic parameters, WebHeaderCollection headers = null, bool PreAuthenticate = false, long languageID = 0)
        {
            SetParameters();
            return Ophelia.URLExtensions.PostObject<T, TResult>(URL, entity, parameters, headers, PreAuthenticate, languageID);
        }
        public static ServiceObjectResult<T> PostObject<T>(this string URL, T entity, WebHeaderCollection headers = null, bool PreAuthenticate = false, long languageID = 0)
        {
            SetParameters();
            return Ophelia.URLExtensions.PostObject<T>(URL, entity, headers, PreAuthenticate, languageID);
        }
        public static ServiceObjectResult<T> GetObject<T>(this string URL, long ID, dynamic parameters = null, WebHeaderCollection headers = null, bool PreAuthenticate = false)
        {
            SetParameters();
            return Ophelia.URLExtensions.GetObject<T>(URL, ID, parameters, headers, PreAuthenticate);
        }
        public static ServiceObjectResult<T> GetObjectByParam<T>(this string URL, dynamic parameters, WebHeaderCollection headers = null, bool PreAuthenticate = false)
        {
            SetParameters();
            return Ophelia.URLExtensions.GetObjectByParam<T>(URL, parameters, headers, PreAuthenticate);
        }
        public static ServiceCollectionResult<T> GetCollection<T>(this string URL, int page, int pageSize, dynamic parameters = null, WebHeaderCollection headers = null, bool PreAuthenticate = false)
        {
            SetParameters();
            return Ophelia.URLExtensions.GetCollection<T>(URL, page, pageSize, parameters, headers, PreAuthenticate);
        }
        public static TResult GetCollection<T, TResult>(this string URL, int page, int pageSize, dynamic parameters = null, WebHeaderCollection headers = null, bool PreAuthenticate = false)
        {
            SetParameters();
            return Ophelia.URLExtensions.GetCollection<T, TResult>(URL, page, pageSize, parameters, headers, PreAuthenticate);
        }
        public static ServiceCollectionResult<T> GetCollection<T>(this string URL, int page, int pageSize, T filterEntity, dynamic parameters = null, WebHeaderCollection headers = null, bool PreAuthenticate = false)
        {
            SetParameters();
            return Ophelia.URLExtensions.GetCollection<T>(URL, page, pageSize, filterEntity, parameters, headers, PreAuthenticate);
        }
        public static TResult GetCollection<T, TResult>(this string URL, int page, int pageSize, T filterEntity, dynamic parameters = null, WebHeaderCollection headers = null, bool PreAuthenticate = false)
        {
            SetParameters();
            return Ophelia.URLExtensions.GetCollection<T, TResult>(URL, page, pageSize, filterEntity, parameters, headers, PreAuthenticate);
        }
        public static TResult GetObject<T, TResult>(this string URL, WebApiObjectRequest<T> request, WebHeaderCollection headers = null, bool PreAuthenticate = false)
        {
            SetParameters();
            return Ophelia.URLExtensions.GetObject<T, TResult>(URL, request, headers, PreAuthenticate);
        }
        public static TResult GetCollection<T, TResult>(this string URL, ServiceCollectionRequest<T> request, WebHeaderCollection headers = null, bool PreAuthenticate = false)
        {
            SetParameters();
            return Ophelia.URLExtensions.GetCollection<T, TResult>(URL, request, headers, PreAuthenticate);
        }
        private static void SetParameters()
        {
            if (FilesToUpload != null && FilesToUpload.Any())
            {
                if (Ophelia.URLExtensions.FilesToUploadBase64 == null)
                    Ophelia.URLExtensions.FilesToUploadBase64 = new List<FileData>();
                foreach (var file in FilesToUpload.Where(op => op.Value != null && op.Value.Length > 0))
                {
                    Ophelia.URLExtensions.FilesToUploadBase64.Add(file.Value.ToFileData(file.Key));
                }
            }
            FilesToUpload = null;
        }
    }
}
