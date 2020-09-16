using System;
using System.Collections.Generic;
using System.Text;

namespace Ophelia.Web.Application.Server.DistributedCaches
{
    public class RedisCacheOptions
    {
        public RedisCacheOptions()
        {
            this.Prefix = "";
            this.ContextName = "Redis";
        }
        public string Prefix { get; set; }
        public string ContextName { get; set; }
    }
}
