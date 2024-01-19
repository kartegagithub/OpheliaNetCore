using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace Ophelia.Integration.GitHub
{
    [DataContract(IsReference = true)]
    public class GitHubCommitResult
    {
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("sha")]
        public string Sha { get; set; } = "";

        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("node_id")]
        public string NodeID { get; set; } = "";

        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("commit")]
        public Commit Commit { get; set; }

        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("url")]
        public string Url { get; set; } = "";

        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; } = "";

        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("comments_url")]
        public string CommentsUrl { get; set; } = "";

        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("author")]
        public MainResultAuthorCommitter Author { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("committer")]
        public MainResultAuthorCommitter Committer { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("parents")]
        public Parent[] Parents { get; set; }
    }

    public class MainResultAuthorCommitter
    {
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("login")]
        public string Login { get; set; } = "";

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
        public string NodeID { get; set; } = "";

        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; set; } = "";

        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("gravatar_id")]
        public string GravatarID { get; set; } = "";

        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("url")]
        public string Url { get; set; } = "";

        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; } = "";

        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("followers_url")]
        public string FollowersUrl { get; set; } = "";

        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("following_url")]
        public string FollowingUrl { get; set; } = "";

        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("gists_url")]
        public string GistsUrl { get; set; } = "";

        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("starred_url")]
        public string StarredUrl { get; set; } = "";

        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("subscriptions_url")]
        public string SubscriptionsUrl { get; set; } = "";

        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("organizations_url")]
        public string OrganizationsUrl { get; set; } = "";

        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("repos_url")]
        public string ReposUrl { get; set; } = "";

        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("events_url")]
        public string EventsUrl { get; set; } = "";

        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("received_events_url")]
        public string ReceivedEventsUrl { get; set; } = "";

        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("type")]
        public string Type { get; set; } = "";

        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("site_admin")]
        public bool SiteAdmin { get; set; }
    }

    public class Commit
    {
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("author")]
        public CommitAuthorAndCommitter Author { get; set; }

        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("committer")]
        public CommitAuthorAndCommitter Committer { get; set; }

        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("message")]
        public string Message { get; set; } = "";

        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("tree")]
        public Tree Tree { get; set; }

        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("url")]
        public string Url { get; set; } = "";

        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("comment_count")]
        public int CommentCount { get; set; }

        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("verification")]
        public Verification Verification { get; set; }
    }

    public class CommitAuthorAndCommitter
    {
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("name")]
        public string Name { get; set; } = "";

        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("email")]
        public string Email { get; set; } = "";

        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("date")]
        public DateTime Date { get; set; }
    }

    public class Tree
    {
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("sha")]
        public string Sha { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class Verification
    {
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("verified")]
        public bool Verified { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("reason")]
        public string Reason { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("signature")]
        public object Signature { get; set; }
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("payload")]
        public object Payload { get; set; }
    }

    public class Parent
    {
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("sha")]
        public string Sha { get; set; }
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
    }

}
