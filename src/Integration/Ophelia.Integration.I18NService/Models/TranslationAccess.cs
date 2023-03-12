using System;
using System.Collections.Generic;
using System.Text;

namespace Ophelia.Integration.I18NService.Models
{
    public class TranslationAccess: IDisposable
    {
        public string Name { get; set; }
        public string CategoryCode { get; set; }
        public int Count { get; set; }

        public void Dispose()
        {
            this.Name = "";
            this.CategoryCode = "";
        }
    }
}
