using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace Ophelia.Integration.GitHub
{
    [DataContract(IsReference = true)]
    public class GitHubRepoResult
    {
        [DataMember]
        [JsonProperty("id")]
        public int ID { get; set; }

        [DataMember]
        [JsonProperty("node_id")]
        public string NodeID { get; set; }

        [DataMember]
        [JsonProperty("name")]
        public string Name { get; set; }

        [DataMember]
        [JsonProperty("full_name")]
        public string FullName { get; set; }

        [DataMember]
        [JsonProperty("_private")]
        public bool Private { get; set; }

        [DataMember]
        [JsonProperty("owner")]
        public GitHubRepoOwner Owner { get; set; }

        [DataMember]
        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; }

        [DataMember]
        [JsonProperty("description")]
        public object Description { get; set; }

        [DataMember]
        [JsonProperty("fork")]
        public bool Fork { get; set; }

        [DataMember]
        [JsonProperty("url")]
        public string Url { get; set; }

        [DataMember]
        [JsonProperty("forks_url")]
        public string ForksUrl { get; set; }

        [DataMember]
        [JsonProperty("keys_url")]
        public string KeysUrl { get; set; }

        [DataMember]
        [JsonProperty("collaborators_url")]
        public string CollaboratorsUrl { get; set; }

        [DataMember]
        [JsonProperty("teams_url")]
        public string TeamsUrl { get; set; }

        [DataMember]
        [JsonProperty("hooks_url")]
        public string HooksUrl { get; set; }

        [DataMember]
        [JsonProperty("issue_events_url")]
        public string IssueEventsUrl { get; set; }

        [DataMember]
        [JsonProperty("events_url")]
        public string EventsUrl { get; set; }

        [DataMember]
        [JsonProperty("assignees_url")]
        public string AssigneesUrl { get; set; }

        [DataMember]
        [JsonProperty("branches_url")]
        public string BranchesUrl { get; set; }

        [DataMember]
        [JsonProperty("tags_url")]
        public string TagsUrl { get; set; }

        [DataMember]
        [JsonProperty("blobs_url")]
        public string BlobsUrl { get; set; }

        [DataMember]
        [JsonProperty("git_tags_url")]
        public string GitTagsUrl { get; set; }

        [DataMember]
        [JsonProperty("git_refs_url")]
        public string GitRefsUrl { get; set; }

        [DataMember]
        [JsonProperty("trees_url")]
        public string TreesUrl { get; set; }

        [DataMember]
        [JsonProperty("statuses_url")]
        public string StatusesUrl { get; set; }

        [DataMember]
        [JsonProperty("languages_url")]
        public string LanguagesUrl { get; set; }

        [DataMember]
        [JsonProperty("stargazers_url")]
        public string StargazersUrl { get; set; }

        [DataMember]
        [JsonProperty("contributors_url")]
        public string ContributorsUrl { get; set; }

        [DataMember]
        [JsonProperty("subscribers_url")]
        public string SubscribersUrl { get; set; }

        [DataMember]
        [JsonProperty("subscription_url")]
        public string SubscriptionUrl { get; set; }

        [DataMember]
        [JsonProperty("commits_url")]
        public string CommitsUrl { get; set; }

        [DataMember]
        [JsonProperty("git_commits_url")]
        public string GitCommitsUrl { get; set; }

        [DataMember]
        [JsonProperty("comments_url")]
        public string CommentsUrl { get; set; }

        [DataMember]
        [JsonProperty("issue_comment_url")]
        public string IssueCommentUrl { get; set; }

        [DataMember]
        [JsonProperty("contents_url")]
        public string ContentsUrl { get; set; }

        [DataMember]
        [JsonProperty("compare_url")]
        public string CompareUrl { get; set; }

        [DataMember]
        [JsonProperty("merges_url")]
        public string MergesUrl { get; set; }
  
        [DataMember]
        [JsonProperty("archive_url")]
        public string ArchiveUrl { get; set; }

        [DataMember]
        [JsonProperty("downloads_url")]
        public string DownloadsUrl { get; set; }

        [DataMember]
        [JsonProperty("issues_url")]
        public string IssuesUrl { get; set; }

        [DataMember]
        [JsonProperty("pulls_url")]
        public string PullsUrl { get; set; }

        [DataMember]
        [JsonProperty("milestones_url")]
        public string MilestonesUrl { get; set; }

        [DataMember]
        [JsonProperty("notifications_url")]
        public string NotificationsUrl { get; set; }

        [DataMember]
        [JsonProperty("labels_url")]
        public string LabelsUrl { get; set; }

        [DataMember]
        [JsonProperty("releases_url")]
        public string ReleasesUrl { get; set; }

        [DataMember]
        [JsonProperty("deployments_url")]
        public string DeploymentsUrl { get; set; }

        [DataMember]
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [DataMember]
        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [DataMember]
        [JsonProperty("pushed_at")]
        public DateTime PushedAt { get; set; }

        [DataMember]
        [JsonProperty("git_url")]
        public string GitUrl { get; set; }

        [DataMember]
        [JsonProperty("SshUrl")]
        public string ssh_url { get; set; }

        [DataMember]
        [JsonProperty("clone_url")]
        public string CloneUrl { get; set; }

        [DataMember]
        [JsonProperty("svn_url")]
        public string SvnUrl { get; set; }

        [DataMember]
        [JsonProperty("homepage")]
        public object HomePage { get; set; }

        [DataMember]
        [JsonProperty("size")]
        public int Size { get; set; }

        [DataMember]
        [JsonProperty("stargazers_count")]
        public int StargazersCount { get; set; }

        [DataMember]
        [JsonProperty("watchers_count")]
        public int WatchersCount { get; set; }

        [DataMember]
        [JsonProperty("language")]
        public string Language { get; set; }

        [DataMember]
        [JsonProperty("has_issues")]
        public bool HasIssues { get; set; }

        [DataMember]
        [JsonProperty("has_projects")]
        public bool HasProjects { get; set; }

        [DataMember]
        [JsonProperty("has_downloads")]
        public bool HasDownloads { get; set; }

        [DataMember]
        [JsonProperty("has_wiki")]
        public bool HasWiki { get; set; }

        [DataMember]
        [JsonProperty("has_pages")]
        public bool HasPages { get; set; }

        [DataMember]
        [JsonProperty("forks_count")]
        public int ForksCount { get; set; }

        [DataMember]
        [JsonProperty("mirror_url")]
        public object MirrorUrl { get; set; }

        [DataMember]
        [JsonProperty("archived")]
        public bool Archived { get; set; }

        [DataMember]
        [JsonProperty("disabled")]
        public bool Disabled { get; set; }

        [DataMember]
        [JsonProperty("open_issues_count")]
        public int OpenIssuesCount { get; set; }

        [DataMember]
        [JsonProperty("license")]
        public object License { get; set; }

        [DataMember]
        [JsonProperty("allow_forking")]
        public bool AllowForking { get; set; }

        [DataMember]
        [JsonProperty("is_template")]
        public bool IsTemplate { get; set; }

        [DataMember]
        [JsonProperty("topics")]
        public object[] Topics { get; set; }

        [DataMember]
        [JsonProperty("visibility")]
        public string Visibility { get; set; }

        [DataMember]
        [JsonProperty("forks")]
        public int Forks { get; set; }

        [DataMember]
        [JsonProperty("open_issues")]
        public int OpenIssues { get; set; }

        [DataMember]
        [JsonProperty("watchers")]
        public int Watchers { get; set; }

        [DataMember]
        [JsonProperty("default_branch")]
        public string DefaultBranch { get; set; }

        [DataMember]
        [JsonProperty("permissions")]
        public GitHubRepoPermissions Permissions { get; set; }
    }

    public class GitHubRepoOwner
    {
        [DataMember]
        [JsonProperty("login")]
        public string Login { get; set; }

        [DataMember]
        [JsonProperty("id")]
        public int ID { get; set; }

        [DataMember]
        [JsonProperty("node_id")]
        public string NodeID { get; set; }

        [DataMember]
        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; set; }

        [DataMember]
        [JsonProperty("gravatar_id")]
        public string GravatarID { get; set; }

        [DataMember]
        [JsonProperty("url")]
        public string Url { get; set; }

        [DataMember]
        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; }

        [DataMember]
        [JsonProperty("followers_url")]
        public string FollowersUrl { get; set; }

        [DataMember]
        [JsonProperty("following_url")]
        public string FollowingUrl { get; set; }

        [DataMember]
        [JsonProperty("gists_url")]
        public string GistsUrl { get; set; }

        [DataMember]
        [JsonProperty("starred_url")]
        public string StarredUrl { get; set; }

        [DataMember]
        [JsonProperty("subscriptions_url")]
        public string SubscriptionsUrl { get; set; }

        [DataMember]
        [JsonProperty("organizations_url")]
        public string OrganizationsUrl { get; set; }

        [DataMember]
        [JsonProperty("repos_url")]
        public string ReposUrl { get; set; }

        [DataMember]
        [JsonProperty("events_url")]
        public string EventsUrl { get; set; }

        [DataMember]
        [JsonProperty("received_events_url")]
        public string ReceivedEventsUrl { get; set; }

        [DataMember]
        [JsonProperty("type")]
        public string Type { get; set; }

        [DataMember]
        [JsonProperty("site_admin")]
        public bool SiteAdmin { get; set; }
    }

    public class GitHubRepoPermissions
    {
        [DataMember]
        [JsonProperty("admin")]
        public bool Admin { get; set; }

        [DataMember]
        [JsonProperty("maintain")]
        public bool Maintain { get; set; }

        [DataMember]
        [JsonProperty("push")]
        public bool Push { get; set; }
  
        [DataMember]
        [JsonProperty("triage")]
        public bool Triage { get; set; }

        [DataMember]
        [JsonProperty("pull")]
        public bool Pull { get; set; }
    }
}