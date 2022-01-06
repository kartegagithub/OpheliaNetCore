using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace Ophelia.Integration.GitHub
{
    [DataContract(IsReference = true)]
    public class GitHubRepoResult
    {
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("id")]
        public int ID { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("node_id")]
        public string NodeID { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("name")]
        public string Name { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("full_name")]
        public string FullName { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("_private")]
        public bool Private { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("owner")]
        public GitHubRepoOwner Owner { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("description")]
        public object Description { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("fork")]
        public bool Fork { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("url")]
        public string Url { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("forks_url")]
        public string ForksUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("keys_url")]
        public string KeysUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("collaborators_url")]
        public string CollaboratorsUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("teams_url")]
        public string TeamsUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("hooks_url")]
        public string HooksUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("issue_events_url")]
        public string IssueEventsUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("events_url")]
        public string EventsUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("assignees_url")]
        public string AssigneesUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("branches_url")]
        public string BranchesUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("tags_url")]
        public string TagsUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("blobs_url")]
        public string BlobsUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("git_tags_url")]
        public string GitTagsUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("git_refs_url")]
        public string GitRefsUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("trees_url")]
        public string TreesUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("statuses_url")]
        public string StatusesUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("languages_url")]
        public string LanguagesUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("stargazers_url")]
        public string StargazersUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("contributors_url")]
        public string ContributorsUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("subscribers_url")]
        public string SubscribersUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("subscription_url")]
        public string SubscriptionUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("commits_url")]
        public string CommitsUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("git_commits_url")]
        public string GitCommitsUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("comments_url")]
        public string CommentsUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("issue_comment_url")]
        public string IssueCommentUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("contents_url")]
        public string ContentsUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("compare_url")]
        public string CompareUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("merges_url")]
        public string MergesUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("archive_url")]
        public string ArchiveUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("downloads_url")]
        public string DownloadsUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("issues_url")]
        public string IssuesUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("pulls_url")]
        public string PullsUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("milestones_url")]
        public string MilestonesUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("notifications_url")]
        public string NotificationsUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("labels_url")]
        public string LabelsUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("releases_url")]
        public string ReleasesUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("deployments_url")]
        public string DeploymentsUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("pushed_at")]
        public DateTime PushedAt { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("git_url")]
        public string GitUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("SshUrl")]
        public string ssh_url { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("clone_url")]
        public string CloneUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("svn_url")]
        public string SvnUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("homepage")]
        public object HomePage { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("size")]
        public int Size { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("stargazers_count")]
        public int StargazersCount { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("watchers_count")]
        public int WatchersCount { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("language")]
        public string Language { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("has_issues")]
        public bool HasIssues { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("has_projects")]
        public bool HasProjects { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("has_downloads")]
        public bool HasDownloads { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("has_wiki")]
        public bool HasWiki { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("has_pages")]
        public bool HasPages { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("forks_count")]
        public int ForksCount { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("mirror_url")]
        public object MirrorUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("archived")]
        public bool Archived { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("disabled")]
        public bool Disabled { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("open_issues_count")]
        public int OpenIssuesCount { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("license")]
        public object License { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("allow_forking")]
        public bool AllowForking { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("is_template")]
        public bool IsTemplate { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("topics")]
        public object[] Topics { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("visibility")]
        public string Visibility { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("forks")]
        public int Forks { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("open_issues")]
        public int OpenIssues { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("watchers")]
        public int Watchers { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("default_branch")]
        public string DefaultBranch { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("permissions")]
        public GitHubRepoPermissions Permissions { get; set; }
    }

    public class GitHubRepoOwner
    {
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("login")]
        public string Login { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("id")]
        public int ID { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("node_id")]
        public string NodeID { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("gravatar_id")]
        public string GravatarID { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("url")]
        public string Url { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("followers_url")]
        public string FollowersUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("following_url")]
        public string FollowingUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("gists_url")]
        public string GistsUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("starred_url")]
        public string StarredUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("subscriptions_url")]
        public string SubscriptionsUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("organizations_url")]
        public string OrganizationsUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("repos_url")]
        public string ReposUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("events_url")]
        public string EventsUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("received_events_url")]
        public string ReceivedEventsUrl { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("type")]
        public string Type { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("site_admin")]
        public bool SiteAdmin { get; set; }
    }

    public class GitHubRepoPermissions
    {
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("admin")]
        public bool Admin { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("maintain")]
        public bool Maintain { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("push")]
        public bool Push { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("triage")]
        public bool Triage { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("pull")]
        public bool Pull { get; set; }
    }
}