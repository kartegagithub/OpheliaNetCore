namespace Ophelia.Web.Routing
{
    public abstract class RouteItemURL
    {
        public string LanguageCode { get; set; }
        public RouteItem RouteItem { get; set; }

        public RouteItemURL()
        {

        }
    }
}
