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
            return Ophelia.Web.Client.Current.Request.Cookies[cookieName];
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
            CookieOptions options = new CookieOptions();
            options.Expires = DateTime.Now.AddMilliseconds(expireMinute);
            Ophelia.Web.Client.Current.Response.Cookies.Append(cookieName, value, options);
        }

        /// <summary>
        /// Verilen Option ile cookie set eder
        /// </summary>
        /// <param name="cookieName"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public static void Set(string cookieName, string value, CookieOptions options)
        {
            Ophelia.Web.Client.Current.Response.Cookies.Append(cookieName, value, options);
        }

        /// <summary>
        /// Verilen isimdeki cookie bilgisini temizler.
        /// </summary>
        public static void ClearByName(string cookieName)
        {
            if (Ophelia.Web.Client.Current != null && Ophelia.Web.Client.Current.Response != null)
            {
                var option = new CookieOptions();
                option.Expires = DateTime.Now.AddDays(-1);
                Ophelia.Web.Client.Current.Response.Cookies.Delete(cookieName, option);
            }
        }
    }
}
