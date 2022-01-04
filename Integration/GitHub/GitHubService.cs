using Newtonsoft.Json;
using Ophelia.Service;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Ophelia.Integration.GitHub
{
    public class GitHubService
    {
        public GitHubService(string ServiceURL, string AuthenticationToken)
        {
            this.ServiceURL = ServiceURL;
            this.AuthenticationToken = AuthenticationToken;
        }

        public string ServiceURL { get; set; }

        public string AuthenticationToken { get; set; }

        public List<GitHubRepoResult> Repos;

        public string reposCommits { get; set; }


        //Kullanıcıya ait tüm repoları döner
        public ServiceCollectionResult<GitHubRepoResult> GetUserGithubRepos()
        {
            var result = new ServiceCollectionResult<GitHubRepoResult>();
            try
            {
                var URL = ServiceURL + "/user/repos";
                var parameters = new { };
                var method = "GET";
                var contentType = "application/x-www-form-urlencoded";
                var headers = new WebHeaderCollection
                {
                    { "Authorization", "Token " + this.AuthenticationToken },
                    { "Accept", "application/vnd.github.v3+json"},
                    { "User-Agent", "KartegaV2"}
                };

                var serviceResult = URL.DownloadURL(method, "", contentType, headers);

                if (!string.IsNullOrEmpty(serviceResult))
                    result.SetData(serviceResult.FromJson<List<GitHubRepoResult>>());
                else
                    result.Fail("AProblemOccurred");

                Repos = result.Data;
            }
            catch (Exception ex)
            {
                result.Fail(ex);
            }
            return result;
        }

        /// <param name="repoOwner">Reponun sahibi</param>
        /// <param name="repoName">Commitlerini görmek istediğimiz reponun adı</param>
        /// <param name="sinceDate">Yalnızca bu tarihten sonraki commitleri getirir </param>
        /// <param name="untilDate">Yalnızca bu tarihten önceki commitleri getirir </param>
        public ServiceCollectionResult<GitHubCommitResult> GetUserGithubSelectedRepoCommits(string repoOwner, string repoName, DateTime sinceDate, DateTime untilDate)
        {
            var result = new ServiceCollectionResult<GitHubCommitResult>();
            try
            {
                var parameters = "since=" + sinceDate.Date.AddHours(00).ToString("O") + "&until=" + untilDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(999).ToString("O") + "";
                var URL = ServiceURL + "/repos" + "/" + repoOwner + "/" + repoName + "/commits?" + parameters;
                var method = "GET";
                var contentType = "application/x-www-form-urlencoded";
                var headers = new WebHeaderCollection
                {
                    { "Authorization", "Token " + this.AuthenticationToken },
                    { "Accept", "application/vnd.github.v3+json"},
                    { "User-Agent", "KartegaV2"}
                };

                var serviceResult = URL.DownloadURL(method, parameters.ToString(), contentType, headers);
                if (!string.IsNullOrEmpty(serviceResult))
                    result.SetData(serviceResult.FromJson<List<GitHubCommitResult>>());
                else
                    result.Fail("AProblemOccurred");
            }
            catch (Exception ex)
            {
                result.Fail(ex);
            }
            return result;
        }

        public ServiceCollectionResult<GitHubCommitResult> GetUserGithubAllRepoCommits(DateTime sinceDate, DateTime untilDate)
        {
            var result = new ServiceCollectionResult<GitHubCommitResult>();
            List<GitHubCommitResult> Commits = new List<GitHubCommitResult>();
            try
            {
                GetUserGithubRepos();
                var parameters = "since=" + sinceDate.Date.AddHours(00).ToString("O") + "&until=" + untilDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(999).ToString("O") + "";
                var URL = "";
                var method = "GET";
                var contentType = "application/x-www-form-urlencoded";
                var headers = new WebHeaderCollection
                {
                    { "Authorization", "Token " + this.AuthenticationToken },
                    { "Accept", "application/vnd.github.v3+json"},
                    { "User-Agent", "KartegaV2"}
                };

                foreach (var repo in Repos)
                {
                    URL = ServiceURL + "/repos" + "/" + repo.FullName + "/commits?" + parameters;
                    var serviceResult = URL.DownloadURL(method, parameters.ToString(), contentType, headers);

                    if (!string.IsNullOrEmpty(serviceResult))
                        result.SetData(serviceResult.FromJson<List<GitHubCommitResult>>());

                    else
                        result.Fail("AProblemOccurred");

                    foreach (var i in result.Data)
                    {
                        Commits.Add(i);
                    }
                }
                result.SetData(Commits);
            }
            catch (Exception ex)
            {
                result.Fail(ex);
            }
            return result;
        }
    }

}
