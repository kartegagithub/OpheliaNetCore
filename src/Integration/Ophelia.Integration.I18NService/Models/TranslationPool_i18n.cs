using System;

namespace Ophelia.Integration.I18NService.Models
{
    public class TranslationPool_i18n : IDisposable
    {
        public string LanguageCode { get; set; }

        public string Description { get; set; }
        public void Dispose()
        {
            this.LanguageCode = "";
            this.Description = "";
        }
    }
}
