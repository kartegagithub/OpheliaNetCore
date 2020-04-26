using System.Collections.Specialized;
using System.Linq;

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
