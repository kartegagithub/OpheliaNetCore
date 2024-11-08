using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Xml.Xsl;

namespace Ophelia.Web.View.Mvc.Middlewares
{
    public class HTTPContextAccessor : IHttpContextAccessor
    {
        public static HttpContext Current
        {
            get
            {
                return _httpContextCurrent?.Value?.Context;
            }
        }
        private static AsyncLocal<HttpContextHolder> _httpContextCurrent = new AsyncLocal<HttpContextHolder>();
        public static void SetContext(HttpContext context)
        {
            _httpContextCurrent.Value = new HttpContextHolder { Context = context };
        }

        public virtual HttpContext? HttpContext
        {
            get
            {
                return _httpContextCurrent.Value?.Context;
            }
            set
            {
                var holder = _httpContextCurrent.Value;
                if (holder != null)
                {
                    holder.Context = null;
                }

                if (value != null)
                {
                    SetContext(value);
                }
            }
        }

        private class HttpContextHolder
        {
            public HttpContext Context;
        }
    }
}