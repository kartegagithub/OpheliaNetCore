using System;
using Novell.Directory.Ldap;
using System.Text;

namespace Ophelia.Integration.LDAP
{
    public static class Extensions
    {
        public static string GetPropertyValue(this LdapEntry result, string key)
        {
            var value = new StringBuilder();
            if (result == null)
                return "";
            if (!string.IsNullOrEmpty(result.DN) && result.DN.IndexOf(key) > -1)
            {
                int equalsIndex, commaIndex;
                string resultString = result.DN.ToString();
                equalsIndex = resultString.IndexOf(key + "=", 1);
                commaIndex = resultString.IndexOf(",", 1);
                if (-1 == equalsIndex)
                    return "";
                
                value.Append(resultString.Substring((equalsIndex + 1), (commaIndex - equalsIndex) - 1));
            }
            return value.ToString();
        }

        public static string GetUserFirstName(this LdapEntry result)
        {
            return Convert.ToString(result.GetPropertyValue(ADProperties.FIRSTNAME));
        }
        public static string GetUserLastName(this LdapEntry result)
        {
            return Convert.ToString(result.GetPropertyValue(ADProperties.LASTNAME));
        }
        public static string GetUserEmail(this LdapEntry result)
        {
            return Convert.ToString(result.GetPropertyValue(ADProperties.EMAILADDRESS));
        }
        
    }
}
