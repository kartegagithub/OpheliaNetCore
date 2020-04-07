using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Web.Routing
{
    public class RouteItem
    {
        public string Name { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public bool IsSecure { get; set; }
        public List<RouteItemFixedURL> FixedURLs { get; set; }
        public List<RouteItemURLPattern> Patterns { get; set; }
        public RouteType Type { get; set; }
        public List<RouteItemURLParameter> Parameters { get; set; }

        public RouteItemURLParameter AddParam(string Name, string Value)
        {
            RouteItemURLParameter Param = this.Parameters.Where(op => op.Name.Equals(Name)).FirstOrDefault();
            if (Param == null)
            {
                Param = new RouteItemURLParameter()
                {
                    Name = Name
                };
                this.Parameters.Add(Param);
            }
            Param.Value = Value;
            return Param;
        }

        public void AddParamsToDictionary(string friendlyUrl, IDictionary<string, object> Dictionary, RouteItemURLPattern Pattern)
        {
            foreach (var item in this.Parameters)
            {
                if (item.Value.IndexOf("{") > -1)
                {
                    int counter = 0;
                    Pattern.SplitPattern();
                    foreach (var p in Pattern.SplittedPattern)
	                {
		                if(p.Equals(item.Value)){
                            var splitted = friendlyUrl.Split('/');
                            for (int i = 0; i < splitted.Length; i++)
			                {
                                if (i == counter)
                                {
                                    Dictionary[item.Name] = splitted[i];
                                    break;
                                }
			                }
                            break;
                        }
                        counter++;
	                }
                }
                else
                    Dictionary[item.Name] = item.Value;
            }
        }

        public void AddParamsToDictionary(string friendlyUrl, IDictionary<string, object> Dictionary)
        {
            foreach (var item in this.Parameters)
            {
                Dictionary[item.Name] = item.Value;
            }
        }

        public RouteItemURLPattern AddPattern(string LanguageCode, string Pattern)
        {
            Pattern = Pattern.Trim('/');
            RouteItemURLPattern Item = this.Patterns.Where(op => op.LanguageCode.Equals(LanguageCode)).FirstOrDefault();
            if (Item == null)
            {
                Item = new RouteItemURLPattern()
                {
                    LanguageCode = LanguageCode
                };
                this.Patterns.Add(Item);
            }
            Item.Pattern = Pattern;
            Item.RouteItem = this;
            return Item;
        }

        public RouteItemFixedURL AddURL(string LanguageCode, string URL) {
            URL = URL.Trim('/');
            RouteItemFixedURL Item = this.FixedURLs.Where(op => op.LanguageCode.Equals(LanguageCode)).FirstOrDefault();
            if (Item == null) {
                Item = new RouteItemFixedURL() { 
                    LanguageCode = LanguageCode
                };
                this.FixedURLs.Add(Item);
            }
            Item.URL = URL;
            Item.RouteItem = this;
            return Item;
        }
        public RouteItem() {
            this.FixedURLs = new List<RouteItemFixedURL>();
            this.Patterns = new List<RouteItemURLPattern>();
            this.Parameters = new List<RouteItemURLParameter>();
        }
    }
}