using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ophelia.Integration.GitHub.Model
{
    [DataContract(IsReference = true)]
    public class GitHubCommitDetailResult
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
        public Author1 Author { get; set; }
        [DataMember]
        [JsonProperty("committer")]
        public Committer1 Committer { get; set; }
        [DataMember]
        [JsonProperty("parents")]
        public Parent[] Parents { get; set; }
        [DataMember]
        [JsonProperty("stats")]
        public Stats Stats { get; set; }
        [DataMember]
        [JsonProperty("files")]
        public List<File> Files { get; set; }
    }

    public class Commit
    {
        [DataMember]
        [JsonProperty("author")]
        public Author Author { get; set; }
        [DataMember]
        [JsonProperty("committer")]
        public Committer Committer { get; set; }
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

    public class Author
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

    public class Committer
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

    public class Author1
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

    public class Committer1
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

    public class Stats
    {
        [DataMember]
        [JsonProperty("total")]
        public int Total { get; set; }
        [DataMember]
        [JsonProperty("additions")]
        public int Additions { get; set; }
        [DataMember]
        [JsonProperty("deletions")]
        public int Deletions { get; set; }
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

    public class File
    {
        [DataMember]
        [JsonProperty("sha")]
        public string Sha { get; set; }
        [DataMember]
        [JsonProperty("filename")]
        public string Filename { get; set; }
        [DataMember]
        [JsonProperty("status")]
        public string Status { get; set; }
        [DataMember]
        [JsonProperty("additions")]
        public int Additions { get; set; }
        [DataMember]
        [JsonProperty("deletions")]
        public int Deletions { get; set; }
        [DataMember]
        [JsonProperty("changes")]
        public int Changes { get; set; }
        [DataMember]
        [JsonProperty("blob_url")]
        public string BlobUrl { get; set; }
        [DataMember]
        [JsonProperty("raw_url")]
        public string RawUrl { get; set; }
        [DataMember]
        [JsonProperty("contents_url")]
        public string ContentsUrl { get; set; }
        [DataMember]
        [JsonProperty("patch")]
        public string Patch { get; set; }
        [DataMember]
        [JsonProperty("previous_filename")]
        public string PreviousFilename { get; set; }
    }

}