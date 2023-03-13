using Ophelia.Service;
using System;
using System.Net;

namespace Ophelia.Integration.I18NService.Services.Base
{
    public class Facade: IDisposable
    {
        public I18NIntegratorClient API { get; private set; }

        protected virtual string Schema { get; }

        public virtual TResult PostObject<T, TResult>(string URL, T entity, dynamic parameters, long languageID = 0)
        {
            return this.ProcessResult(URLExtensions.PostObject<T, TResult>(this.API.ServiceURL + "/" + this.Schema + "/" + URL, entity, parameters, this.GetHeaders(URL), true, languageID));
        }

        public virtual ServiceObjectResult<T> PostObject<T>(string URL, T entity, long languageID = 0)
        {
            return (ServiceObjectResult<T>)this.ProcessResult(URLExtensions.PostObject<T>(this.API.ServiceURL + "/" + this.Schema + "/" + URL, entity, this.GetHeaders(URL), true, languageID));
        }

        public virtual ServiceObjectResult<T> GetObject<T>(string URL, long ID, dynamic parameters = null)
        {
            return this.ProcessResult(URLExtensions.GetObject<T>(this.API.ServiceURL + "/" + this.Schema + "/" + URL, ID, parameters, this.GetHeaders(URL), true));
        }

        public virtual ServiceObjectResult<T> GetObject<T>(string URL, T entity)
        {
            return (ServiceObjectResult<T>)this.ProcessResult(URLExtensions.PostObject<T>(this.API.ServiceURL + "/" + this.Schema + "/" + URL, entity, this.GetHeaders(URL), true));
        }

        public virtual ServiceObjectResult<T> GetObject<T>(string URL, ServiceObjectRequest<T> request)
        {
            return (ServiceObjectResult<T>)this.ProcessResult(URLExtensions.GetObject(this.API.ServiceURL + "/" + this.Schema + "/" + URL, request, this.GetHeaders(URL), true));
        }

        public virtual ServiceCollectionResult<T> GetCollection<T>(string URL, int page, int pageSize, dynamic parameters = null)
        {
            return this.ProcessResult(URLExtensions.GetCollection<T>(this.API.ServiceURL + "/" + this.Schema + "/" + URL, page, pageSize, parameters, this.GetHeaders(URL), true));
        }
        public virtual TResult GetCollection<T, TResult>(string URL, int page, int pageSize, dynamic parameters = null)
        {
            return this.ProcessResult(URLExtensions.GetCollection<T, TResult>(this.API.ServiceURL + "/" + this.Schema + "/" + URL, page, pageSize, parameters, this.GetHeaders(URL), true));
        }
        public virtual ServiceCollectionResult<T> GetCollection<T>(string URL, int page, int pageSize, T filterEntity, dynamic parameters = null)
        {
            return this.ProcessResult(URLExtensions.GetCollection<T>(this.API.ServiceURL + "/" + this.Schema + "/" + URL, page, pageSize, filterEntity, parameters, this.GetHeaders(URL), true));
        }
        public virtual TResult GetCollection<T, TResult>(string URL, int page, int pageSize, T filterEntity, dynamic parameters = null)
        {
            return this.ProcessResult(URLExtensions.GetCollection<T, TResult>(this.API.ServiceURL + "/" + this.Schema + "/" + URL, page, pageSize, filterEntity, parameters, this.GetHeaders(URL), true));
        }
        public virtual ServiceCollectionResult<T> GetCollection<T>(string URL, ServiceCollectionRequest<T> request)
        {
            return (ServiceCollectionResult<T>)this.ProcessResult(URLExtensions.GetCollection<T, ServiceCollectionResult<T>>(this.API.ServiceURL + "/" + this.Schema + "/" + URL, request, this.GetHeaders(URL), true));
        }

        public T PostURL<T>(string URL, dynamic parameters, string contentType = "application/x-www-form-urlencoded")
        {
            return this.ProcessResult(Ophelia.URLExtensions.PostURL<T>(this.API.ServiceURL + "/" + this.Schema + "/" + URL, parameters, this.GetHeaders(URL), true));
        }

        private object ProcessResult(object obj)
        {
            return obj;
        }

        private WebHeaderCollection GetHeaders(string URL)
        {
            var headers = new WebHeaderCollection();
            headers.Add("AppKey", this.API.AppKey);
            headers.Add("AppCode", this.API.AppCode);
            headers.Add("AppName", this.API.AppName);
            headers.Add("ProjectCode", this.API.ProjectCode);
            headers.Add("ProjectName", this.API.ProjectName);
            return headers;
        }

        void IDisposable.Dispose()
        {
            URLExtensions.FilesToUploadBase64 = null;
        }

        public Facade(I18NIntegratorClient API)
        {
            this.API = API;
        }
    }
}