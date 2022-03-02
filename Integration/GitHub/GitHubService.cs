﻿using Newtonsoft.Json;
using Ophelia.Integration.GitHub;
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

        private string ServiceURL { get; set; }

        private string AuthenticationToken { get; set; }

        private WebHeaderCollection Headers()
        {
            return new WebHeaderCollection
                {
                    { "Authorization", "Token " + this.AuthenticationToken },
                    { "Accept", "application/vnd.github.v3+json"},
                    { "User-Agent", "KartegaV2"}
                };
        }

        /// <summary>
        /// Kullanıcıya ait tüm repoları döner
        /// </summary>
        /// <param name="page"> </param>
        /// <param name="pageSize">max 100 için değer döner </param>
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
        /// Repoya ait commitleri döner
        /// </summary>
        /// <param name="repoOwner">Reponun sahibi</param>
        /// <param name="repoName">Commitlerini görmek istediğimiz reponun adı</param>
        /// <param name="startDate">Yalnızca bu tarihten sonraki commitleri getirir </param>
        /// <param name="endDate">Yalnızca bu tarihten önceki commitleri getirir </param>
        /// <param name="page"> </param>
        /// <param name="pageSize">max 100 için değer döner </param>
        public ServiceCollectionResult<GitHubCommitResult> GetRepoCommits(string repoOwner, string repoName, DateTime startDate, DateTime endDate, string branchCommitSha, int page = 1, int pageSize = 100)
        {
            var result = new ServiceCollectionResult<GitHubCommitResult>();
            try
            {
                var URL = $"{this.ServiceURL}/repos/{repoOwner}/{repoName}/commits?since={startDate.ToUniversalTime().ToString("s")}Z&until={endDate.ToUniversalTime().ToString("s")}Z";

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
        /// Repoya ait branchları döner
        /// </summary>
        /// <param name="repoOwner">Reponun sahibi</param>
        /// <param name="repoName">Commitlerini görmek istediğimiz reponun adı</param>
        /// <param name="page"> </param>
        /// <param name="pageSize">max 100 için değer döner </param>
        public ServiceCollectionResult<GitHubRepoBranchResult> GetRepoBranches(string repoOwner, string repoName,  int page = 1, int pageSize = 100)
        {
            var result = new ServiceCollectionResult<GitHubRepoBranchResult>();
            try
            {
                var URL = $"{this.ServiceURL}/repos/{repoOwner}/{repoName}/branches";

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
    }

}
