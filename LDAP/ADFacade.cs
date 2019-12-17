using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ophelia;
using System.Text.RegularExpressions;

namespace Ophelia.LDAP
{
    public abstract class ADFacade
    {
        public static SearchResultCollection FindUsers(string userName, params string[] properties)
        {
            var search = GetUserDirectorySearcher(userName, properties);
            return search.FindAll();
        }
        public static SearchResult FindUser(string userName, params string[] properties)
        {
            var search = GetUserDirectorySearcher(userName, properties);
            return search.FindOne();
        }
        private static DirectorySearcher GetUserDirectorySearcher(string userName, params string[] properties)
        {
            if (userName.IndexOf("\\") > -1)
            {
                userName = userName.Replace(userName.Left(userName.IndexOf("\\")) + "\\", "");
            }
            var entry = new DirectoryEntry();
            DirectorySearcher search = new DirectorySearcher(entry);
            search.Filter = "(&(objectClass=user)(anr=" + userName + "))";

            if(properties != null)
            {
                foreach (var item in properties)
                {
                    search.PropertiesToLoad.Add(item);
                }
            }
            return search;
        }
        public static bool IsAuthenticated(string domain, string username, string pwd, string path)
        {
            string whitelist = @"^[a-zA-Z\-\.']$";
            var pattern = new Regex(whitelist);

            if (!pattern.IsMatch(domain) && !pattern.IsMatch(username))
            {
                string domainAndUsername = domain + @"\" + username;
                try
                {
                    using (DirectoryEntry entry = new DirectoryEntry(path, domainAndUsername, pwd))
                    {
                        object obj = entry.NativeObject;
                        using (var search = new DirectorySearcher(entry))
                        {
                            search.Filter = string.Format("(SAMAccountName={0})", username);
                            search.PropertiesToLoad.Add("cn");
                            SearchResult result = search.FindOne();

                            if (result == null)
                                return false;

                            result = null;
                            obj = null;
                        }
                    }
                }
                catch
                {
                    return false;
                }
                return true;
            }
            return false;
        }
    }
}
