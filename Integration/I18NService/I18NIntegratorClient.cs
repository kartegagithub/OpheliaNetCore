using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ophelia.Integration.I18NService.Models;
using Ophelia;
using Ophelia.Service;

namespace Ophelia.Integration.I18NService
{
    public class I18NIntegratorClient : IDisposable
    {
        [ThreadStatic]
        internal static I18NIntegratorClient _Current;
        public static I18NIntegratorClient Current
        {
            get
            {
                if (_Current == null)
                    _Current = new I18NIntegratorClient();
                return _Current;
            }
        }
        public string ServiceURL { get; set; }
        private Services.IntegrationService Service { get; set; }
        public string AppCode { get; set; }
        public string AppName { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public string AppKey { get; set; }
        public List<TranslationAccess> Accesses { get; set; }
        public void AccessedToTranslation(string name)
        {
            if (this.Accesses == null)
                this.Accesses = new List<TranslationAccess>();

            var access = this.Accesses.FirstOrDefault(op => op.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (access == null)
                this.Accesses.Add(new TranslationAccess() { Name = name, Count = 1 });
            else
                access.Count += 1;
        }
        public void Flush()
        {
            if (string.IsNullOrEmpty(this.ServiceURL))
                return;
            if (string.IsNullOrEmpty(this.AppKey))
                return;
            if (this.Accesses != null && this.Accesses.Any() && this.Service != null && !string.IsNullOrEmpty(this.ServiceURL))
                this.Service.ProcessAccesses(this.Accesses);
        }
        public ServiceObjectResult<Models.TranslationPool> GetTranslation(string name)
        {
            var result = new ServiceObjectResult<Models.TranslationPool>();
            try
            {
                if (string.IsNullOrEmpty(this.ServiceURL))
                {
                    result.Fail("Invalid service url");
                    return result;
                }
                if (string.IsNullOrEmpty(this.AppKey))
                {
                    result.Fail("Invalid app key");
                    return result;
                }
                result = this.Service.GetTranslation(name);
            }
            catch (Exception ex)
            {
                result.Fail(ex);
            }
            return result;
        }
        public ServiceCollectionResult<Models.TranslationPool> GetUpdates()
        {
            var result = new ServiceCollectionResult<Models.TranslationPool>();
            try
            {
                if (string.IsNullOrEmpty(this.ServiceURL))
                {
                    result.Fail("Invalid service url");
                    return result;
                }
                if (string.IsNullOrEmpty(this.AppKey))
                {
                    result.Fail("Invalid app key");
                    return result;
                }
                result = this.Service.GetUpdates();
            }
            catch (Exception ex)
            {
                result.Fail(ex);
            }
            return result;
        }
        public void Init(string serviceURL, string appCode, string appName, string projectCode, string projectName, string appKey)
        {
            this.ServiceURL = serviceURL;
            this.AppCode = appCode;
            this.AppName = appName;
            this.AppKey = appKey;
            this.ProjectCode = projectCode;
            this.ProjectName = projectName;
            this.Service = new Services.IntegrationService(this);
            this.CheckURL();
        }
        private void CheckURL()
        {
            if (!string.IsNullOrEmpty(this.ServiceURL))
            {
                if (this.ServiceURL.IndexOf("api") == -1)
                    this.ServiceURL = this.ServiceURL.TrimEnd('/') + "/api";
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            this.Accesses = null;
            this.ServiceURL = "";
            this.Service = null;
            this.AppCode = "";
            this.AppKey = "";
            this.AppName = "";
            this.ProjectCode = "";
            this.ProjectName = "";
            _Current = null;
        }
    }
}
