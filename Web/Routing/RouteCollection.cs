using System;
using System.Collections.Generic;
using System.Linq;

namespace Ophelia.Web.Routing
{
    public class RouteCollection : List<RouteItem>
    {
        public Client Client
        {
            get
            {
                return Client.Current;
            }
        }

        public RouteItem AddPattern(string Name, string Pattern, string LanguageCode, string ParameterName, string ParameterValue, bool IsSecure = false)
        {
            var Item = this.AddPattern(Name, Pattern, LanguageCode, IsSecure);
            Item.AddParam(ParameterName, ParameterValue);
            return Item;
        }
        public RouteItem Add(string Name, string URL, string LanguageCode, string ParameterName, string ParameterValue, bool IsSecure = false)
        {
            if (!string.IsNullOrEmpty(Name))
            {
                var Item = this.Add(Name, URL, LanguageCode, IsSecure);
                Item.AddParam(ParameterName, ParameterValue);
                return Item;
            }
            else
            {
                throw new Exception("Name parameter of route can not be null for URL: " + URL);
            }
        }
        public RouteItem AddPattern(string Name, string Pattern, string LanguageCode, bool IsSecure = false)
        {
            if (!string.IsNullOrEmpty(Name))
            {
                RouteItem Item = this.Where(op => op.Name.Equals(Name)).FirstOrDefault();

                if (Item == null)
                {
                    Item = new RouteItem()
                    {
                        Name = Name,
                        Type = RouteType.Auto,
                        IsSecure = IsSecure
                    };
                    this.Add(Item);
                }
                Item.AddPattern(LanguageCode, Pattern);
                return Item;
            }
            else
            {
                throw new Exception("Name parameter of route can not be null for Pattern: " + Pattern);
            }
        }

        public RouteItem Add(string Name, string URL, string LanguageCode, bool IsSecure = false)
        {
            if (!string.IsNullOrEmpty(Name))
            {
                RouteItem Item = this.Where(op => op.Name.Equals(Name)).FirstOrDefault();

                if (Item == null)
                {
                    Item = new RouteItem()
                    {
                        Name = Name,
                        Type = RouteType.Custom,
                        IsSecure = IsSecure
                    };
                    this.Add(Item);
                }
                Item.AddURL(LanguageCode, URL);
                return Item;
            }
            else
            {
                throw new Exception("Name parameter of route can not be null for URL: " + URL);
            }
        }

        #region "GetRootItem"
        public virtual RouteItemURL GetRootItem(string langCode, string Name, string Controller, string Action, string Parameter1, string Value1, string Parameter2, string Value2, string Parameter3, string Value3, string Parameter4, string Value4, string Parameter5, string Value5, bool AbsolutePath = false)
        {
            List<RouteItem> Routes = null;
            if (!string.IsNullOrEmpty(Name))
                Routes = this.Where(op => !string.IsNullOrEmpty(op.Name) && op.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase)).ToList();
            else
                Routes = this.Where(op => op.Controller == Controller && op.Action == Action).ToList();

            if (Routes != null && Routes.Count > 1)
            {
                if (!string.IsNullOrEmpty(Parameter1))
                {
                    Routes = Routes.Where(op => op.Parameters.Where(op2 => op2.Name == Parameter1 && op2.Value == Value1).Any()).ToList();
                }
                if (!string.IsNullOrEmpty(Parameter2))
                {
                    Routes = Routes.Where(op => op.Parameters.Where(op2 => op2.Name == Parameter2 && op2.Value == Value2).Any()).ToList();
                }
                if (!string.IsNullOrEmpty(Parameter3))
                {
                    Routes = Routes.Where(op => op.Parameters.Where(op2 => op2.Name == Parameter3 && op2.Value == Value3).Any()).ToList();
                }
                if (!string.IsNullOrEmpty(Parameter4))
                {
                    Routes = Routes.Where(op => op.Parameters.Where(op2 => op2.Name == Parameter4 && op2.Value == Value4).Any()).ToList();
                }
                if (!string.IsNullOrEmpty(Parameter5))
                {
                    Routes = Routes.Where(op => op.Parameters.Where(op2 => op2.Name == Parameter5 && op2.Value == Value5).Any()).ToList();
                }
            }
            if (Routes != null && Routes.Count > 0)
            {
                var temp = Routes.Where(op => op.Patterns.Where(op2 => op2.LanguageCode == langCode && op2.Pattern != "").Any()).ToList();
                if (temp.Count > 0)
                {
                    return temp.FirstOrDefault().Patterns.Where(op => op.LanguageCode == langCode && op.Pattern != "").FirstOrDefault();
                }
                else
                {
                    temp = Routes.Where(op => op.FixedURLs.Where(op2 => op2.LanguageCode == langCode && op2.URL != "").Any()).ToList();
                    if (temp.Count > 0)
                    {
                        return temp.FirstOrDefault().FixedURLs.Where(op => op.LanguageCode == langCode && op.URL != "").FirstOrDefault();
                    }
                }
                temp = Routes.Where(op => op.Patterns.Where(op2 => op2.Pattern != "").Any()).ToList();
                if (temp.Count > 0)
                {
                    return temp.FirstOrDefault().Patterns.Where(op => op.Pattern != "").FirstOrDefault();
                }
                else
                {
                    temp = Routes.Where(op => op.FixedURLs.Where(op2 => op2.URL != "").Any()).ToList();
                    if (temp.Count > 0)
                    {
                        return temp.FirstOrDefault().FixedURLs.Where(op => op.URL != "").FirstOrDefault();
                    }
                }
            }
            return null;
        }
        public virtual RouteItemURL GetRootItem(string langCode, string Controller, string Action, string Parameter1, string Value1, string Parameter2, string Value2, string Parameter3, string Value3, string Parameter4, string Value4, bool AbsolutePath = false)
        {
            return this.GetRootItem(langCode, "", Controller, Action, Parameter1, Value1, Parameter2, Value2, Parameter3, Value3, Parameter4, Value4, "", "", AbsolutePath);
        }
        public virtual RouteItemURL GetRootItem(string langCode, string Controller, string Action, string Parameter1, string Value1, string Parameter2, string Value2, string Parameter3, string Value3, bool AbsolutePath = false)
        {
            return this.GetRootItem(langCode, Controller, Action, Parameter1, Value1, Parameter2, Value2, Parameter3, Value3, "", "", AbsolutePath);
        }
        public virtual RouteItemURL GetRootItem(string langCode, string Controller, string Action, string Parameter1, string Value1, string Parameter2, string Value2, bool AbsolutePath = false)
        {
            return this.GetRootItem(langCode, Controller, Action, Parameter1, Value1, Parameter2, Value2, "", "", AbsolutePath);
        }
        public virtual RouteItemURL GetRootItem(string langCode, string Controller, string Action, string Parameter1, string Value1, bool AbsolutePath = false)
        {
            return this.GetRootItem(langCode, Controller, Action, Parameter1, Value1, "", "", AbsolutePath);
        }
        public virtual RouteItemURL GetRootItem(string langCode, string Controller, string Action, bool AbsolutePath = false)
        {
            return this.GetRootItem(langCode, Controller, Action, "", "", AbsolutePath);
        }
        public virtual RouteItemURL GetRootItem(string langCode, string Name, bool AbsolutePath = false)
        {
            return this.GetRootItem(langCode, Name, "", "", "", "", "", "", "", "", "", "", "", "", AbsolutePath);
        }
        public virtual RouteItemURL GetRootItem(string langCode, string Name, string Parameter1, string Value1, bool AbsolutePath = false)
        {
            return this.GetRootItem(langCode, Name, "", "", Parameter1, Value1, "", "", "", "", "", "", "", "", AbsolutePath);
        }
        public virtual RouteItemURL GetRootItem(string langCode, string Name, string Parameter1, string Value1, string Parameter2, string Value2, bool AbsolutePath = false)
        {
            return this.GetRootItem(langCode, Name, "", "", Parameter1, Value1, Parameter2, Value2, "", "", "", "", "", "", AbsolutePath);
        }
        public virtual RouteItemURL GetRootItem(string langCode, string Name, string Parameter1, string Value1, string Parameter2, string Value2, string Parameter3, string Value3, bool AbsolutePath = false)
        {
            return this.GetRootItem(langCode, Name, "", "", Parameter1, Value1, Parameter2, Value2, Parameter3, Value3, "", "", "", "", AbsolutePath);
        }
        public virtual RouteItemURL GetRootItemByName(string langCode, string Name, bool AbsolutePath = false)
        {
            return this.GetRootItem(langCode, Name, "", "", "", "", "", "", "", "", "", "", "", "", AbsolutePath);
        }
        public virtual RouteItemURL GetRootItemByName(string langCode, string Name, string Parameter1, string Value1, bool AbsolutePath = false)
        {
            return this.GetRootItem(langCode, Name, "", "", Parameter1, Value1, "", "", "", "", "", "", "", "", AbsolutePath);
        }
        public virtual RouteItemURL GetRootItemByName(string langCode, string Name, string Parameter1, string Value1, string Parameter2, string Value2, bool AbsolutePath = false)
        {
            return this.GetRootItem(langCode, Name, "", "", Parameter1, Value1, Parameter2, Value2, "", "", "", "", "", "", AbsolutePath);
        }
        public virtual RouteItemURL GetRootItemByName(string langCode, string Name, string Parameter1, string Value1, string Parameter2, string Value2, string Parameter3, string Value3, bool AbsolutePath = false)
        {
            return this.GetRootItem(langCode, Name, "", "", Parameter1, Value1, Parameter2, Value2, Parameter3, Value3, "", "", "", "", AbsolutePath);
        }

        #endregion "GetRootItem"

        #region "GetURL"

        protected virtual void OnURLNotFound(string Name, string Controller, string Action)
        {

        }
        public virtual string GetURL(string langCode, string Name, string Controller, string Action, string Parameter1, string Value1, string Parameter2, string Value2, string Parameter3, string Value3, string Parameter4, string Value4, string Parameter5, string Value5, bool AbsolutePath = false)
        {
            string URL = "";
            RouteItem Item = null;
            RouteItemURL routeURL = this.GetRootItem(langCode, Name, Controller, Action, Parameter1, Value1, Parameter2, Value2, Parameter3, Value3, Parameter4, Value4, Parameter5, Value5, AbsolutePath);
            if (routeURL != null)
            {
                Item = routeURL.RouteItem;
                if (routeURL is RouteItemFixedURL)
                    URL = (routeURL as RouteItemFixedURL).URL;
                else
                    URL = (routeURL as RouteItemURLPattern).Pattern;
            }
            else
                this.OnURLNotFound(Name, Controller, Action);
            
            var returnURL = "";
            if (!returnURL.Equals("/"))
                returnURL = "/" + URL;
            else
                returnURL = "/";

            if (Item != null && Item.IsSecure && Client.Current.Request.Host.ToString().IndexOf("localhost") == -1)
            {
                returnURL = "https://" + Client.Current.Request.Host.ToString() + returnURL;
            }
            else if (Item != null && AbsolutePath)
            {
                returnURL = Client.Current.Request.Scheme + "://" + Client.Current.Request.Host.ToString() + returnURL;
            }

            return returnURL;
        }
        public virtual string GetURL(string langCode, string Controller, string Action, string Parameter1, string Value1, string Parameter2, string Value2, string Parameter3, string Value3, string Parameter4, string Value4, bool AbsolutePath = false)
        {
            return this.GetURL(langCode, "", Controller, Action, Parameter1, Value1, Parameter2, Value2, Parameter3, Value3, Parameter4, Value4, "", "", AbsolutePath);
        }
        public virtual string GetURL(string langCode, string Controller, string Action, string Parameter1, string Value1, string Parameter2, string Value2, string Parameter3, string Value3, bool AbsolutePath = false)
        {
            return this.GetURL(langCode, Controller, Action, Parameter1, Value1, Parameter2, Value2, Parameter3, Value3, "", "", AbsolutePath);
        }
        public virtual string GetURL(string langCode, string Controller, string Action, string Parameter1, string Value1, string Parameter2, string Value2, bool AbsolutePath = false)
        {
            return this.GetURL(langCode, Controller, Action, Parameter1, Value1, Parameter2, Value2, "", "", AbsolutePath);
        }
        public virtual string GetURL(string langCode, string Controller, string Action, string Parameter1, string Value1, bool AbsolutePath = false)
        {
            return this.GetURL(langCode, Controller, Action, Parameter1, Value1, "", "", AbsolutePath);
        }
        public virtual string GetURL(string langCode, string Controller, string Action, bool AbsolutePath = false)
        {
            return this.GetURL(langCode, Controller, Action, "", "", AbsolutePath);
        }
        public virtual string GetURLByName(string langCode, string Name, bool AbsolutePath = false)
        {
            return this.GetURL(langCode, Name, "", "", "", "", "", "", "", "", "", "", "", "", AbsolutePath);
        }
        public virtual string GetURLByName(string langCode, string Name, string Parameter1, string Value1, bool AbsolutePath = false)
        {
            return this.GetURL(langCode, Name, "", "", Parameter1, Value1, "", "", "", "", "", "", "", "", AbsolutePath);
        }
        public virtual string GetURLByName(string langCode, string Name, string Parameter1, string Value1, string Parameter2, string Value2, bool AbsolutePath = false)
        {
            return this.GetURL(langCode, Name, "", "", Parameter1, Value1, Parameter2, Value2, "", "", "", "", "", "", AbsolutePath);
        }
        public virtual string GetURLByName(string langCode, string Name, string Parameter1, string Value1, string Parameter2, string Value2, string Parameter3, string Value3, bool AbsolutePath = false)
        {
            return this.GetURL(langCode, Name, "", "", Parameter1, Value1, Parameter2, Value2, Parameter3, Value3, "", "", "", "", AbsolutePath);
        }
        public virtual string GetURL(string langCode, object value, bool AbsolutePath = false)
        {
            return "";
        }
        #endregion "GetURL"

        public virtual RouteItemURL GetPatternURL(string friendlyUrl, string languageCode = "en")
        {
            var items = friendlyUrl.Split('/');
            if (items.Length >= 2)
            {
                var URL = this.Where(op => op.Patterns.Where(op2 => op2.Pattern.StartsWith(items[0] + "/", StringComparison.InvariantCultureIgnoreCase) && !op.FixedURLs.Any()).Any()).ToList();
                if (URL.Count > 1)
                {
                    var tempPattern = this.GetPatterns(URL, items[1]);
                    if (tempPattern != null)
                    {
                        URL = tempPattern;

                        if (URL.Count > 1 && items.Length >= 3)
                        {
                            tempPattern = this.GetPatterns(URL, items[2]);
                            if (tempPattern != null)
                            {
                                URL = tempPattern;

                                if (URL.Count > 1 && items.Length >= 4)
                                {
                                    tempPattern = this.GetPatterns(URL, items[3]);
                                    if (tempPattern != null)
                                    {
                                        URL = tempPattern;

                                        if (URL.Count > 1 && items.Length >= 5)
                                        {
                                            tempPattern = this.GetPatterns(URL, items[4]);
                                            if (tempPattern != null)
                                            {
                                                URL = tempPattern;

                                                if (URL.Count > 1 && items.Length >= 6)
                                                {
                                                    tempPattern = this.GetPatterns(URL, items[5]);
                                                    if (tempPattern != null)
                                                    {
                                                        URL = tempPattern;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (URL.Count > 0)
                {
                    var routeItems = URL.FirstOrDefault().Patterns.Where(op2 => op2.Pattern.StartsWith(items[0] + "/", StringComparison.InvariantCultureIgnoreCase)).ToList();
                    if (routeItems.Count == 1)
                        return routeItems.FirstOrDefault();
                    else if (items.Length >= 2)
                    {
                        var urls = routeItems.Where(op => op.Pattern.IndexOf("/" + items[1], StringComparison.InvariantCultureIgnoreCase) > -1).ToList();
                        if (urls.Count > 1 && !string.IsNullOrEmpty(languageCode))
                            return urls.Where(op => op.LanguageCode == languageCode).FirstOrDefault();
                        else
                            return urls.FirstOrDefault();
                    }
                }
            }
            return null;
        }

        private List<RouteItem> GetPatterns(List<RouteItem> Parent, string Item)
        {
            return Parent.Where(op => op.Patterns.Where(op2 => op2.Pattern.IndexOf("/" + Item, StringComparison.InvariantCultureIgnoreCase) > -1).Any()).ToList();
        }

        public virtual RouteItemURL GetFixedURL(string friendlyUrl, string languageCode = "en")
        {
            var URL = this.Where(op => op.FixedURLs.Where(op2 => op2.URL.Equals(friendlyUrl, StringComparison.InvariantCultureIgnoreCase) && !op.Patterns.Any()).Any()).FirstOrDefault();
            if (URL != null)
            {
                if (!string.IsNullOrEmpty(URL.Controller))
                {
                    var urls = URL.FixedURLs.Where(op => op.URL.Equals(friendlyUrl, StringComparison.InvariantCultureIgnoreCase)).ToList();
                    if (urls.Count > 1 && !string.IsNullOrEmpty(languageCode))
                        return urls.Where(op => op.LanguageCode == languageCode).FirstOrDefault();
                    else
                        return urls.FirstOrDefault();
                }
            }
            return null;
        }

        public virtual RouteItemURL GetFixedURLFromAction(string nonFriendlyUrl, string languageCode = "en")
        {
            if (string.IsNullOrEmpty(nonFriendlyUrl))
                return null;
            var items = nonFriendlyUrl.Trim('/').Split('/');
            var area = "";
            var controller = "";
            var action = "";
            if (items.Length == 3)
            {
                area = items[0];
                controller = items[1];
                action = items[2];
            }
            else if (items.Length == 2)
            {
                controller = items[0];
                action = items[1];
            }
            var URL = this.Where(op => op.Controller.Equals(controller, StringComparison.InvariantCultureIgnoreCase) && op.Action.Equals(action, StringComparison.InvariantCultureIgnoreCase) && op.Area.Equals(area, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (URL != null)
            {
                if (!string.IsNullOrEmpty(URL.Controller))
                {
                    var urls = URL.FixedURLs.Where(op => op.LanguageCode == languageCode).FirstOrDefault();
                    if (urls == null)
                        return URL.FixedURLs.FirstOrDefault();
                    else
                        return urls;
                }
            }
            return null;
        }
    }
}
