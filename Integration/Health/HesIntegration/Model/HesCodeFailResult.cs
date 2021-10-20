using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Ophelia.Integration.Health
{
    [DataContract(IsReference = true)]
    public class HesCodeFailResult
    {
        [DataMember]
        [JsonProperty("entityName")]
        public string EntityName { get; set; }

        [DataMember]
        [JsonProperty("errorKey")]
        public string ErrorKey { get; set; }

        [DataMember]
        [JsonProperty("type")]
        public string Type { get; set; }

        [DataMember]
        [JsonProperty("title")]
        public string Title { get; set; }

        [DataMember]
        [JsonProperty("status")]
        public int Status { get; set; }

        [DataMember]
        [JsonProperty("message")]
        public string Message { get; set; }

        [DataMember]
        [JsonProperty("params")]
        public string Params { get; set; }

        [DataMember]
        [JsonProperty("detail")]
        public string Detail { get; set; }
    }
}
