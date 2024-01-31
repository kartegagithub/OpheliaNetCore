using System;
using Novell.Directory.Ldap;
using System.Text;

namespace Ophelia.Integration.LDAP
{
    public static class Extensions
    {
        public static string GetPropertyValue(this LdapEntry result, string key)
        {
			if (result == null)
				return "";
            return result.getAttribute(key)?.StringValue;
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
		public static string GetLoginName(this LdapEntry result)
		{
			return Convert.ToString(result.GetPropertyValue(ADProperties.LOGINNAME));
		}
		public static string GetServicePrincipalName(this LdapEntry result)
		{
			return Convert.ToString(result.GetPropertyValue(ADProperties.SERVICEPRINCIPALNAME));
		}
		public static string GetUserPrincipalName(this LdapEntry result)
		{
			return Convert.ToString(result.GetPropertyValue(ADProperties.USERPRINCIPALNAME));
		}
		public static string[] GetMemberOf(this LdapEntry result)
		{
			return result.getAttribute(ADProperties.MEMBEROF).StringValueArray;
		}
		public static bool IsActive(this LdapEntry entry, int flag = 0x0002)
		{
			try
			{
				LdapAttribute attributeAcc = entry.getAttribute("userAccountControl");
				var flags = Convert.ToInt32(attributeAcc.StringValue);
				return !Convert.ToBoolean(flags & flag);
			}
			catch (Exception)
			{
				return true;
			}
		}
	}
}
