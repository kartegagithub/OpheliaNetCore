using System;

namespace Ophelia.Integration.I18NService.Models
{
    public class TranslationPoolProject : IDisposable
    {
        public string Name { get; set; }

        public string ProjectCode { get; set; }
        public void Dispose()
        {
            this.Name = "";
            this.ProjectCode = "";
        }
    }
}
