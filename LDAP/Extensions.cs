using System;
using System.Collections.Generic;
using System.DirectoryServices;

namespace Ophelia.LDAP
{
    public static class Extensions
    {
        public static object GetPropertyValue(this SearchResult result, string key)
        {
            if (result.Properties.Contains(key) && result.Properties[key].Count > 0)
                return result.Properties[key][0];
            return "";
        }
        public static ResultPropertyValueCollection GetPropertyValues(this SearchResult result, string key)
        {
            if (result.Properties.Contains(key) && result.Properties[key].Count > 0)
                return result.Properties[key];
            return null;
        }
        public static string GetUserFirstName(this SearchResult result)
        {
            return Convert.ToString(result.GetPropertyValue(ADProperties.FIRSTNAME));
        }
        public static string GetUserLastName(this SearchResult result)
        {
            return Convert.ToString(result.GetPropertyValue(ADProperties.LASTNAME));
        }
        public static string GetUserEmail(this SearchResult result)
        {
            return Convert.ToString(result.GetPropertyValue(ADProperties.EMAILADDRESS));
        }
        public static List<string> GetUserMemberOf(this SearchResult result)
        {
            var list = new List<string>();

            var values = result.GetPropertyValues("memberof");
            if (values != null)
            {
                foreach (var item in values)
                {
                    list.Add(Convert.ToString(item).Split(',')[0].Replace("CN=", ""));
                }
            }
            return list;
        }
    }
}
