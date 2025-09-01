using Microsoft.AspNetCore.Hosting;

namespace Ophelia.Web.Application
{
    public class Host
    {
        public static IWebHost Current { get; set; }
    }
    public static class HostExtensions
    {
        public static IWebHost SetCurrent(this IWebHost host)
        {
            Host.Current = host;
            return host;
        }
    }
}
