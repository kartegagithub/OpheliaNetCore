using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Web.View.Mvc.Controls.Binders.EntityBinder
{
    public class HelpConfiguration: IDisposable
    {
        public bool Enable { get; set; }
        public string ClassName { get; set; }
        public Dictionary<string, string> Documentation { get; set; }
        public List<string> DiscardedDocumentation { get; set; }
        public List<SearchHelp> SearchHelps { get; set; }
        public HelpConfiguration()
        {
            this.Documentation = new Dictionary<string, string>();
            this.DiscardedDocumentation = new List<string>();
            this.SearchHelps = new List<SearchHelp>();
        }

        public void Dispose()
        {
            this.SearchHelps.Clear();
            this.SearchHelps = null;
            this.Documentation.Clear();
            this.Documentation = null;
            this.DiscardedDocumentation.Clear();
            this.DiscardedDocumentation = null;
        }
    }
}
