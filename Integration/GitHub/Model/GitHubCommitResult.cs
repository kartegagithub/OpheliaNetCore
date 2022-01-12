using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace Ophelia.Integration.GitHub
{
    [DataContract(IsReference = true)]
    public class GitHubCommitResult
    {
        [DataMember]
        [JsonProperty("sha")]
        public string Sha { get; set; }

        [DataMember]
        [JsonProperty("node_id")]
        public string NodeID { get; set; }

        [DataMember]
        [JsonProperty("commit")]
        public Commit Commit { get; set; }

        [DataMember]
        [JsonProperty("url")]
        public string Url { get; set; }      
        
        [DataMember]
        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; }

        [DataMember]
        [JsonProperty("comments_url")]
        public string CommentsUrl { get; set; }

        [DataMember]
        [JsonProperty("author")]
        public MainResultAuthorCommitter Author { get; set; }

        [DataMember]
        [JsonProperty("committer")]
        public MainResultAuthorCommitter Committer { get; set; }

        [DataMember]
        [JsonProperty("parents")]
        public Parent[] Parents { get; set; }
    }

    public class MainResultAuthorCommitter
    {
        [DataMember]
        [JsonProperty("login")]
        public string Login { get; set; }

        [DataMember]
        [JsonProperty("id")]
        public int ID { get; set; }

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

    public class Commit
    {
        [DataMember]
        [JsonProperty("author")]
        public CommitAuthorAndCommitter Author { get; set; }
        
        [DataMember]
        [JsonProperty("committer")]
        public CommitAuthorAndCommitter Committer { get; set; }
        
        [DataMember]
        [JsonProperty("message")]
        public string Message { get; set; }
        
        [DataMember]
        [JsonProperty("tree")]
        public Tree Tree { get; set; }
        
        [DataMember]
        [JsonProperty("url")]
        public string Url { get; set; }
        
        [DataMember]
        [JsonProperty("comment_count")]
        public int CommentCount { get; set; }
        
        [DataMember]
        [JsonProperty("verification")]
        public Verification Verification { get; set; }
    }

    public class CommitAuthorAndCommitter
    {
        [DataMember]
        [JsonProperty("name")]
        public string Name { get; set; }

        [DataMember]
        [JsonProperty("email")]
        public string Email { get; set; }

        [DataMember]
        [JsonProperty("date")]
        public DateTime Date { get; set; }
    }

    public class Tree
    {
        [DataMember]
        [JsonProperty("sha")]
        public string Sha { get; set; }
        
        [DataMember]
        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class Verification
    {
        [DataMember]
        [JsonProperty("verified")]
        public bool Verified { get; set; }
        
        [DataMember]
        [JsonProperty("reason")]
        public string Reason { get; set; }

        [DataMember]
        [JsonProperty("signature")]
        public object Signature { get; set; }

        [DataMember]
        [JsonProperty("payload")]
        public object Payload { get; set; }
    }

    public class Parent
    {
        [DataMember]
        [JsonProperty("sha")]
        public string Sha { get; set; }
        
        [DataMember]
        [JsonProperty("url")]
        public string Url { get; set; }
        
        [DataMember]
        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; }
    }

}
