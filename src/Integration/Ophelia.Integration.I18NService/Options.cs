namespace Ophelia.Integration.I18NService
{
    public class Options
    {
        public string ServiceURL { get; set; } = "https://i18napi.kartega.com";
        public string AppCode { get; set; }
        public string AppName { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public string AppKey { get; set; }
        public Options()
        {

        }
    }
}
