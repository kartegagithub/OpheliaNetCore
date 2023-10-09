using System;
using System.Collections.Generic;
using System.Text;

namespace Ophelia.Integration.I18NService.Models
{
    public class TranslationPoolValidatationModel : IDisposable
    {
        public ICollection<TranslationPoolProject> Translations { get; set; }

        public void Dispose()
        {
            this.Translations = null;
        }
    }
}
