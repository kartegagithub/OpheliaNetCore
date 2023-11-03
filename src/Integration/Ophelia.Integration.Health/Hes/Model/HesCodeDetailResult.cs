using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace Ophelia.Integration.Health.Hes
{
    [DataContract(IsReference = true)]
    public class HesCodeDetailResult : HesCodeResult
    {
        /// <summary>
        /// HES kodu sahibine ait COVID-19 aşılama durumunu dönmektedir. 2. Doz aşı uygulanan HES kodu sahiplerine ait sonuç "true" dönmektedir. COVID-19 aşı uygulanmamış veya dozları tamamlanmamış kişiler de "false/null" olarak dönmektedir
        /// </summary>
        [DataMember]
        [JsonProperty("is_vaccinated")]
        public Nullable<bool> IsVaccinated { get; set; }

        /// <summary>
        /// HES kodu sahibine ait son 6 ay içerisinde COVID-19 hastalığı durumunu dönmektedir. Son 6 ay içerisinde COVID-19 hastalığı geçirmiş HES kodu sahiplerine ait sonuç "true" dönmektedir. Son 6 ay içerisinde COVID-19 hastalığı geçirmemiş veya COVID-19 hastalığını 6 aydan daha fazla süre önce geçirmiş kişiler de "false/null" olarak dönmektedir
        /// </summary>
        [DataMember]
        [JsonProperty("is_immune")]
        public Nullable<bool> IsImmune { get; set; }

        /// <summary>
        /// HES kodu sahibine ait son 72 saat içerisinde COVID-19 PCR negatif test sonucunu dönmektedir. Son 72 saat içerisinde COVID-19 PCR negatif test sonucu olan HES kodu sahiplerine ait sonuç "2021-08-09T11:51:30.597Z" formatında dönmektedir. Son 72 saat içerisinde COVID-19 PCR negatif test sonucu olmayan kişiler de "null" olarak dönmektedir
        /// </summary>
        [DataMember]
        [JsonProperty("last_negative_test_date")]
        public Nullable<DateTime> LastNegativeTestDate { get; set; }
    }
}
