using Microsoft.AspNetCore.Http;
using System;

namespace Ophelia.Web.Application.Client
{
    public static class CookieManager
    {
        /// <summary>
        /// Verilen isimdeki cookie bilgisini döner.
        /// </summary>
        public static string Get(string cookieName)
        {
            if (Web.Client.Current.Context != null)
                return Web.Client.Current.Request.Cookies[cookieName];
            else
                return "";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cookieName"></param>
        /// <param name="value"></param>
        /// <param name="expireMinute"></param>
        /// <returns></returns>
        public static void Set(string cookieName, string value, int expireMinute = 10)
        {
            if (Web.Client.Current.Context != null)
            {
                CookieOptions options = new CookieOptions();
                options.Expires = Ophelia.Utility.Now.AddMilliseconds(expireMinute);
                Web.Client.Current.Response.Cookies.Append(cookieName, value, options);
            }
        }

        /// <summary>
        /// Verilen Option ile cookie set eder
        /// </summary>
        /// <param name="cookieName"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public static void Set(string cookieName, string value, CookieOptions options)
        {
            if (Web.Client.Current.Context != null)
                Web.Client.Current.Response.Cookies.Append(cookieName, value, options);
        }

        /// <summary>
        /// Verilen isimdeki cookie bilgisini temizler.
        /// </summary>
        public static void ClearByName(string cookieName, CookieOptions options = null)
        {
            if (Web.Client.Current != null && Web.Client.Current.Context != null)
            {
                if (options == null)
                    options = new CookieOptions();
                options.Expires = Ophelia.Utility.Now.AddDays(-1);
                Web.Client.Current.Response.Cookies.Delete(cookieName, options);
            }
        }
    }
}
