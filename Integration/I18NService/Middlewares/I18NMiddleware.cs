using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ophelia.Integration.I18NService.Middlewares
{
    public class I18NMiddleware
    {
        public static I18NIntegratorClient I18NClient { get; private set; }
        private readonly RequestDelegate _next;
        public I18NMiddleware(RequestDelegate next, Options options = null)
        {
            _next = next;
            if (I18NClient == null)
            {
                I18NClient = new I18NIntegratorClient();
                if (options != null && !string.IsNullOrEmpty(options.ServiceURL) && !string.IsNullOrEmpty(options.AppKey))
                {
                    I18NClient.Init(options.ServiceURL, options.AppCode, options.AppName, options.ProjectCode, options.ProjectName, options.AppKey);
                }
            }
        }

        public Task Invoke(HttpContext context)
        {
            try
            {
                return BeginInvoke(context);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (I18NClient != null && I18NClient.Accesses != null && I18NClient.Accesses.Any() && !string.IsNullOrEmpty(I18NClient.AppKey))
                {
                    I18NClient.FlushAsynch().ConfigureAwait(false);
                }
            }
        }
        private Task BeginInvoke(HttpContext context)
        {
            return _next.Invoke(context);
        }
    }
}
