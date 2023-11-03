using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace Ophelia.Integration.Health.Hes
{
    [DataContract(IsReference = true)]
    public class HesCodeResult : HesCodeFailResult
    {
        /// <summary>
        /// Sorgulanan HES Kodu
        /// </summary>
        [DataMember]
        public string HesCode { get; set; } = "";

        /// <summary>
        /// Oluşturulan HES Kodunun son kullanım tarihi
        /// </summary>
        [DataMember]
        [JsonProperty("expiration_date")]
        public DateTime ExpirationDate { get; set; }

        /// <summary>
        /// Kişinin sorgulama anındaki risk durumu
        /// </summary>
        [DataMember]
        [JsonProperty("current_health_status")]
        public string HealthStatus { get; set; } = "";

        /// <summary>
        /// Kişinin adının maskelenmiş halini belirtmektedir
        /// </summary>
        [DataMember]
        [JsonProperty("masked_firstname")]
        public string MaskedFirstname { get; set; } = "";

        /// <summary>
        /// Kişinin kimlik numarasının maskelenmiş halini belirtmektedir
        /// </summary>
        [DataMember]
        [JsonProperty("masked_identity_number")]
        public string MaskedIdentityNumber { get; set; } = "";

        /// <summary>
        /// Kişinin soyadının maskelenmiş halini belirtmektedir
        /// </summary>
        [DataMember]
        [JsonProperty("masked_lastname")]
        public string MaskedLastname { get; set; } = "";
    }
}
