namespace Ophelia.Web.Routing
{
    public class RouteItemURLPattern : RouteItemURL
    {
        public string Pattern { get; set; }
        public string[] SplittedPattern { get; set; }

        public void SplitPattern()
        {
            if (this.SplittedPattern == null)
                this.SplittedPattern = this.Pattern.Split('/');
        }
    }
}
