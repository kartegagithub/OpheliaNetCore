using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Web.Routing
{
    public abstract class RouteItemURL
    {
        public string LanguageCode { get; set; }
        public RouteItem RouteItem { get; set; }
        
        public RouteItemURL() {
            
        }
    }
}
