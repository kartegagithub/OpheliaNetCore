using Microsoft.AspNetCore.Routing;

namespace Ophelia.Web.Routing
{
    public abstract class CustomRouteHandler
    {
        public RouteHandler RouteHandler { get; set; }
        public abstract RouteItem Handle(RouteContext context, string friendlyURL, out bool handled);
    }
}
