using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Threading.Tasks;

namespace Ophelia
{
    public static class NameValueCollectionExtensions
    {
        public static string ToQueryString(this NameValueCollection target)
        {
            return string.Join("&", target.Cast<string>().Select(e => e + "=" + target[e]));
        }
    }
}
