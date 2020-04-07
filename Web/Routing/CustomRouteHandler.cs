using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Ophelia.Web.Routing;

namespace Ophelia.Web.Routing
{
    public abstract class CustomRouteHandler
    {
        public RouteHandler RouteHandler { get; set; }
        public abstract RouteItem Handle(RouteContext context, string friendlyURL, out bool handled);
    }
}
