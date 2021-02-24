using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Ophelia.Integration.I18NService.Models
{
    public class TranslationPool_i18n: IDisposable
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
