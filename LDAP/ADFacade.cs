using Ophelia.Service;
using System;
using System.Text;
using System.Text.RegularExpressions;
using Novell.Directory.Ldap;
using DocumentFormat.OpenXml.Spreadsheet;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using Novell.Directory.Ldap.Utilclass;
using System.Runtime.CompilerServices;

namespace Ophelia.LDAP
{
    public abstract class ADFacade
    {
        public static ServiceCollectionResult<LdapEntry> FindUsers(string domain, string path, string ldapUsername, string ldapPassword, params string[] properties)
        {
            var Result = new ServiceCollectionResult<LdapEntry>();
            using (LdapConnection ldapConnection = CreateConnection(path,domain, ldapUsername, ldapPassword))
            {
                int searchScope = LdapConnection.SCOPE_SUB;
                List<LdapEntry> searchResults =
                ldapConnection.Search($"dc={domain},dc=com",      // container to search
                searchScope,     // search scope
                $"(&(objectClass=User)(objectCategory=Person))",    // search filter
                new[] { LdapConnection.NO_ATTRS },           // "1.1" returns entry name only
                true).ToList();  // no attributes are returned

                Result.SetData(searchResults);
            }
            return Result;
        }



        public static ServiceObjectResult<LdapEntry> FindUser(string domain, string path, string ldapUsername, string ldapPassword, string requestedUserName, params string[] properties)
        {
            var Result = new ServiceObjectResult<LdapEntry>();
            using (LdapConnection ldapConnection = CreateConnection(path , domain, ldapUsername, ldapPassword))
            {
                int searchScope = LdapConnection.SCOPE_SUB;
                LdapEntry searchResults =
                ldapConnection.Search($"dc={domain},dc=com", // container to search
                searchScope,// search scope
                $"(SAMAccountName ={requestedUserName})",    // search filter
                new[] { LdapConnection.NO_ATTRS },           // "1.1" returns entry name only
                true).First();  // no attributes are returned
                Result.SetData(searchResults);
            }
            return Result;
        }
        public static string GetGroups(string domain, string path, string ldapUsername, string ldapPassword, string filterAttribute)
        {
            var groupNames = new StringBuilder();
            try
            {
                using (LdapConnection ldapConnection = CreateConnection(path, domain, ldapUsername, ldapPassword))
                {
                    int searchScope = LdapConnection.SCOPE_SUB;
                    string filter = "(&(objectClass=group)";
                    if (!string.IsNullOrEmpty(filterAttribute))
                        filter += "(cn=" + filterAttribute + "))";
                    List<LdapEntry> searchResults = ldapConnection.Search(
                        $"dc={domain},dc=com", // container
                        searchScope,// search scope
                        filter,// search filter
                        new[] { LdapConnection.NO_ATTRS },// attributes
                        true).ToList();  // no attributes are returned

                    if (searchResults.Count > 0)
                    {
                        int equalsIndex, commaIndex;
                        foreach (var searchResult in searchResults)
                        {
                            string result = searchResult.ToString();

                            equalsIndex = result.IndexOf("=", 1);
                            commaIndex = result.IndexOf(",", 1);
                            if (-1 == equalsIndex)
                            {
                                return null;
                            }
                            groupNames.Append(result.Substring((equalsIndex + 1), (commaIndex - equalsIndex) - 1));
                            groupNames.Append("|");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return groupNames.ToString();
        }


        public static bool IsAuthenticated(string domain, string path, string ldapUserName, string ldapPassword,  string requestedUserName)
        {
            string whitelist = @"^[a-zA-Z\-\.']$";
            var pattern = new Regex(whitelist);

            if (!pattern.IsMatch(domain) && !pattern.IsMatch(ldapUserName))
            {
                string domainAndUsername = domain + @"\" + ldapUserName;
                try
                {
                    using (LdapConnection ldapConnection = CreateConnection(path, domain, ldapUserName, ldapPassword))
                    {
                        int searchScope = LdapConnection.SCOPE_SUB;
                        LdapSearchResults searchResults =
                        ldapConnection.Search($"dc={domain},dc=com", // container to search
                        searchScope, // search scope
                        $"(SAMAccountName ={requestedUserName})", // search filter
                        new[] { LdapConnection.NO_ATTRS }, //attributes
                        true);  // no attributes are returned

                        return searchResults.First() != null;
                    }
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        public static ServiceObjectResult<LdapEntry> GetAuthenticatedUserInfo(string domain , string path, string username, string pwd,  string requestedUserName = "")
        {
            var Result = new ServiceObjectResult<LdapEntry>();
            string whitelist = @"^[a-zA-Z\-\.']$";
            var pattern = new Regex(whitelist);

            if (!pattern.IsMatch(domain) && !pattern.IsMatch(username))
            {
                if (string.IsNullOrEmpty(requestedUserName))
                    requestedUserName = username;

                string domainAndUsername = domain + @"\" + username;
                try
                {
                    using (LdapConnection ldapConnection = CreateConnection(path, domain, username, pwd))
                    {
                        int searchScope = LdapConnection.SCOPE_SUB;
                        LdapSearchResults searchResults =
                        ldapConnection.Search($"dc={domain},dc=com", // container to search
                        searchScope,     // search scope
                        $"(SAMAccountName ={requestedUserName})", // search filter
                        new[] { LdapConnection.NO_ATTRS }, // "1.1" returns entry name only
                        true);  // no attributes are returned

                        Result.SetData(searchResults.First());
                    }
                }
                catch (Exception ex)
                {
                    Result.Fail(ex);
                }
            }
            return Result;
        }

        public static LdapConnection CreateConnection(string path, string domain, string username, string password)
        {
            string whitelist = @"^[a-zA-Z\-\.']$";
            var pattern = new Regex(whitelist);
            if (!pattern.IsMatch(domain) && !pattern.IsMatch(username))
            {
                LdapConnection ldapConnection = new LdapConnection();
                Novell.Directory.Ldap.LdapSearchConstraints cons = ldapConnection.SearchConstraints;
                cons.ReferralFollowing = true;
                ldapConnection.Constraints = cons;
                ldapConnection.Connect(path, LdapConnection.DEFAULT_PORT);
                var sdn = ldapConnection.GetSchemaDN();
                ldapConnection.Bind(username + "@" + domain, password);
                return ldapConnection;
            }
            return null;
        }
    }
}
