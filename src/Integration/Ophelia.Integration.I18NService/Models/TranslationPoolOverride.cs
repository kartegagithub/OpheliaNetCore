using System;

namespace Ophelia.Integration.I18NService.Models
{
    public class TranslationPoolOverride : IDisposable
    {
        public string LanguageCode { get; set; }
        public string ProjectCode { get; set; }
        public string Description { get; set; }
        public void Dispose()
        {
            this.LanguageCode = "";
            this.ProjectCode = "";
            this.Description = "";
        }
    }
}
