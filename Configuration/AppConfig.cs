using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ophelia.Configuration
{
    public class AppConfig
    {
        public static IConfiguration Current { get; set; }
    }
}
