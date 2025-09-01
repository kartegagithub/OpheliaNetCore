using System;
using System.Collections.Generic;

namespace Ophelia.Integration.I18NService.Models
{
    public class TranslationAccessRequest : IDisposable
    {
        public List<TranslationAccess> Accesses { get; set; }

        public void Dispose()
        {
            this.Accesses = null;
        }
    }
}
