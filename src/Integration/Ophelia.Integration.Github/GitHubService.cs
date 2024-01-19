using Ophelia.Integration.GitHub.Model;
using Ophelia.Service;
using System;
using System.Collections.Generic;
using System.Net;

namespace Ophelia.Integration.GitHub
{
    public class GitHubService : IDisposable
    {
        public string UserAgent { get; set; } = "Ophelia/GithubIntegrator";
        public string Accept { get; set; } = "application/vnd.github+json";
        public string AuthorizationType { get; set; } = "Bearer";
        public string ApiVersion { get; set; } = "2022-11-28";
        public GitHubService(string ServiceURL, string AuthenticationToken)
        {
            this.ServiceURL = ServiceURL;
            this.AuthenticationToken = AuthenticationToken;
        }

        private string ServiceURL { get; set; }

        private string AuthenticationToken { get; set; }

        private WebHeaderCollection Headers()
        {
            return new WebHeaderCollection
                {
                    { "Authorization", $"{this.AuthorizationType} {this.AuthenticationToken}" },
                    { "Accept", this.Accept},
                    { "X-GitHub-Api-Version", this.ApiVersion},
                    { "User-Agent", this.UserAgent},
                    { "Traceid", new Guid().ToString()}
                };
        }

        /// <summary>
        /// All repos associated with token user
        /// </summary>
        /// <param name="page">Page (default 1)</param>
        /// <param name="pageSize">PageSize (default 100)</param>
        /// <returns></returns>
        public ServiceCollectionResult<GitHubRepoResult> GetRepos(int page = 1, int pageSize = 100)
        {
            var result = new ServiceCollectionResult<GitHubRepoResult>();
            try
            {
                var URL = $"{this.ServiceURL}/user/repos";
                if (page > 0 && pageSize > 0)
                {
                    URL += $"?page={page}&per_page={pageSize}";
                }

                var serviceResult = URL.DownloadURL("GET", "", "application/x-www-form-urlencoded", this.Headers());

                if (!string.IsNullOrEmpty(serviceResult))
                    result.SetData(serviceResult.FromJson<List<GitHubRepoResult>>());
                else
                    result.Fail("AProblemOccurred");
            }
            catch (Exception ex)
            {
                result.Fail(ex);
            }
            return result;
        }

        /// <summary>
        /// All commits associated with repository
        /// </summary>
        /// <param name="repoOwner">Repo owner</param>
        /// <param name="repoName">Repo name</param>
        /// <param name="startDate">Commits after</param>
        /// <param name="endDate">Commits until</param>
        /// <param name="page">Page</param>
        /// <param name="pageSize">PageSize</param>
        public ServiceCollectionResult<GitHubCommitResult> GetRepoCommits(string repoOwner, string repoName, DateTime startDate, DateTime endDate, string branchCommitSha, int page = 1, int pageSize = 100)
        {
            var result = new ServiceCollectionResult<GitHubCommitResult>();
            try
            {
                var URL = $"{this.ServiceURL}/repos/{FormatName(repoOwner)}/{FormatName(repoName)}/commits?since={startDate.ToUniversalTime().ToString("s")}Z&until={endDate.ToUniversalTime().ToString("s")}Z";

                if (!string.IsNullOrEmpty(branchCommitSha))
                {
                    URL += $"&sha={branchCommitSha.EncodeURL()}";
                }
                if (page > 0 && pageSize > 0)
                {
                    URL += $"&page={page}&per_page={pageSize}";
                }

                var serviceResult = URL.DownloadURL("GET", "", "application/x-www-form-urlencoded", this.Headers());
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

        /// <summary>
        /// All branches associated with repository
        /// </summary>
        /// <param name="repoOwner">Reponun sahibi</param>
        /// <param name="repoName">Commitlerini görmek istediğimiz reponun adı</param>
        /// <param name="page"> </param>
        /// <param name="pageSize">max 100 için değer döner </param>
        public ServiceCollectionResult<GitHubRepoBranchResult> GetRepoBranches(string repoOwner, string repoName, int page = 1, int pageSize = 100)
        {
            var result = new ServiceCollectionResult<GitHubRepoBranchResult>();
            try
            {
                var URL = $"{this.ServiceURL}/repos/{FormatName(repoOwner)}/{FormatName(repoName)}/branches";

                if (page > 0 && pageSize > 0)
                {
                    URL += $"?page={page}&per_page={pageSize}";
                }
                var serviceResult = URL.DownloadURL("GET", "", "application/x-www-form-urlencoded", this.Headers());
                if (!string.IsNullOrEmpty(serviceResult))
                    result.SetData(serviceResult.FromJson<List<GitHubRepoBranchResult>>());
                else
                    result.Fail("AProblemOccurred");
            }
            catch (Exception ex)
            {
                result.Fail(ex);
            }
            return result;
        }

        /// <summary>
        /// Branch details associated with repository
        /// </summary>
        /// <param name="repoOwner">Reponun sahibi</param>
        /// <param name="repoName">Commitlerini görmek istediğimiz reponun adı</param>
        /// <param name="page"> </param>
        /// <param name="pageSize">max 100 için değer döner </param>
        public ServiceObjectResult<GitHubRepoBranchResult> GetRepoBranch(string repoOwner, string repoName, string branchName)
        {
            var result = new ServiceObjectResult<GitHubRepoBranchResult>();
            try
            {
                var URL = $"{this.ServiceURL}/repos/{FormatName(repoOwner)}/{FormatName(repoName)}/branches/{FormatName(branchName)}";
                var serviceResult = URL.DownloadURL("GET", "", "application/x-www-form-urlencoded", this.Headers());
                if (!string.IsNullOrEmpty(serviceResult))
                    result.SetData(serviceResult.FromJson<GitHubRepoBranchResult>());
                else
                    result.Fail("AProblemOccurred");
            }
            catch (Exception ex)
            {
                result.Fail(ex);
            }
            return result;
        }
        /// <summary>
        /// All commits (with details) associated with repository
        /// </summary>
        /// <param name="repoOwner">Reponun sahibi</param>
        /// <param name="repoName">Commitlerini görmek istediğimiz reponun adı</param>
        /// <param name="commitSha">Yalnızca bu tarihten sonraki commitleri getirir </param>
        public ServiceObjectResult<GitHubCommitDetailResult> GetRepoCommitDetail(string repoOwner, string repoName, string commitSha)
        {
            var result = new ServiceObjectResult<GitHubCommitDetailResult>();
            try
            {
                var URL = $"{this.ServiceURL}/repos/{FormatName(repoOwner)}/{FormatName(repoName)}/commits/{FormatName(commitSha)}";

                var serviceResult = URL.DownloadURL("GET", "", "application/x-www-form-urlencoded", this.Headers());
                if (!string.IsNullOrEmpty(serviceResult))
                    result.SetData(serviceResult.FromJson<GitHubCommitDetailResult>());
                else
                    result.Fail("AProblemOccurred");
            }
            catch (Exception ex)
            {
                result.Fail(ex);
            }
            return result;
        }
        private static string FormatName(string name)
        {
            return name.Replace("#", "%23");
        }
        public void Dispose()
        {
            this.UserAgent = "";
            this.Accept = "";
            this.ServiceURL = "";
            this.ApiVersion = "";
            GC.SuppressFinalize(this);
        }
    }

}
