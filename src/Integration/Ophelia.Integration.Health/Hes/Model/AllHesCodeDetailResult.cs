using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ophelia.Integration.Health.Hes
{
    [DataContract(IsReference = true)]
    public class AllHesCodeDetailResult : HesCodeFailResult
    {
        /// <summary>
        /// Sorgulanan hes kodlarından başarılı sonuç dönenler
        /// </summary>
        [DataMember]
        [JsonProperty("success_map")]
        public Dictionary<string, HesCodeDetailResult> SuccessMap { get; set; }

        /// <summary>
        /// Sorgulanan hes kodlarından başarısız sonuç dönenler
        /// </summary>
        [DataMember]
        [JsonProperty("unsuccess_map")]
        public Dictionary<string, string> UnSuccessMap { get; set; }
    }
}
