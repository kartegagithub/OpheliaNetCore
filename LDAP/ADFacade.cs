using Ophelia.Service;
using System;
using System.DirectoryServices;
using System.Text;
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
        public static string GetGroups(string path, string filterAttribute)
        {
            var search = new DirectorySearcher(path)
            {
                Filter = "(cn=" + filterAttribute + ")"
            };
            search.PropertiesToLoad.Add("memberOf");
            var groupNames = new StringBuilder();

            try
            {
                var result = search.FindOne();

                int propertyCount = result.Properties["memberOf"].Count;

                string dn;
                int equalsIndex, commaIndex;

                for (int propertyCounter = 0; propertyCounter < propertyCount; propertyCounter++)
                {
                    dn = (string)result.Properties["memberOf"][propertyCounter];

                    equalsIndex = dn.IndexOf("=", 1);
                    commaIndex = dn.IndexOf(",", 1);
                    if (-1 == equalsIndex)
                    {
                        return null;
                    }

                    groupNames.Append(dn.Substring((equalsIndex + 1), (commaIndex - equalsIndex) - 1));
                    groupNames.Append("|");

                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error obtaining group names. " + ex.Message);
            }
            return groupNames.ToString();
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

            if (properties != null)
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

        public static ServiceObjectResult<SearchResult> GetAuthenticatedUserInfo(string domain, string username, string pwd, string path, string requestedUserName = "")
        {
            var Result = new ServiceObjectResult<SearchResult>();
            string whitelist = @"^[a-zA-Z\-\.']$";
            var pattern = new Regex(whitelist);

            if (!pattern.IsMatch(domain) && !pattern.IsMatch(username))
            {
                if (string.IsNullOrEmpty(requestedUserName))
                    requestedUserName = username;

                string domainAndUsername = domain + @"\" + username;
                try
                {
                    using (DirectoryEntry entry = new DirectoryEntry(path, domainAndUsername, pwd))
                    {
                        object obj = entry.NativeObject;
                        using (var search = new DirectorySearcher(entry))
                        {
                            search.Filter = string.Format("(SAMAccountName={0})", requestedUserName);
                            //search.PropertiesToLoad.Add("cn");
                            obj = null;
                            Result.SetData(search.FindOne());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Result.Fail(ex);
                }
            }
            return Result;
        }
    }
}
