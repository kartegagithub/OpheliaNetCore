using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Ophelia.Integration.GitHub
{
    [DataContract(IsReference = true)]
    public class GitHubRepoBranchResult
    {
        [DataMember]
        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [DataMember]
        [JsonProperty("commit")]
        public GitHubRepoBranchCommit? Commit { get; set; }

        [DataMember]
        [JsonProperty("protected")]
        public bool Protected { get; set; }
    }
    public class GitHubRepoBranchCommit : GitHubCommitResult
    {

    }
}
