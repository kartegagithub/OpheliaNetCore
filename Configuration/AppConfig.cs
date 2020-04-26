using Microsoft.Extensions.Configuration;

namespace Ophelia.Configuration
{
    public class AppConfig
    {
        public static IConfiguration Current { get; set; }
    }
}
