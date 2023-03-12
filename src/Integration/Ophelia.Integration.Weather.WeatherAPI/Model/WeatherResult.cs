using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Ophelia.Integration.Weather.WeatherAPI.Model
{
    [DataContract(IsReference = true)]
    public class WeatherResult
    {
        /// <summary>
        /// Lokasyon Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("location")]
        public WeatherLocation Location { get; set; }

        /// <summary>
        /// Hava durumu Bilgileri
        /// </summary>
        [DataMember]
        [JsonProperty("current")]
        public WeatherCurrent Current { get; set; }
    }

    public class WeatherLocation
    {
        /// <summary>
        /// Adı
        /// </summary>
        [DataMember]
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Bölge
        /// </summary>
        [DataMember]
        [JsonProperty("region")]
        public string Region { get; set; }

        /// <summary>
        /// Ülke
        /// </summary>
        [DataMember]
        [JsonProperty("country")]
        public string Country { get; set; }

        /// <summary>
        /// Enlem
        /// </summary>
        [DataMember]
        [JsonProperty("lat")]
        public float Latitude { get; set; }

        /// <summary>
        /// Boylam
        /// </summary>
        [DataMember]
        [JsonProperty("lon")]
        public float Longitude { get; set; }

        /// <summary>
        /// Timezone adı
        /// </summary>
        [DataMember]
        [JsonProperty("tz_id")]
        public string Timezone { get; set; }

        /// <summary>
        /// Local zaman 
        /// </summary>
        [DataMember]
        [JsonProperty("localtime_epoch")]
        public long LocaltimeUnixTime { get; set; }

        /// <summary>
        /// Local zaman 
        /// </summary>
        [DataMember]
        [JsonProperty("localtime")]
        public string Localtime { get; set; }
    }

    public class WeatherCurrent
    {
        /// <summary>
        /// Son güncelleme zamanı 
        /// </summary>
        [DataMember]
        [JsonProperty("last_updated_epoch")]
        public int LastUpdatedUnixTime { get; set; }

        /// <summary>
        /// Son güncelleme zamanı 
        /// </summary>
        [DataMember]
        [JsonProperty("last_updated")]
        public string LastUpdated { get; set; }

        /// <summary>
        /// Santigrat cinsinden sıcaklık
        /// </summary>
        [DataMember]
        [JsonProperty("temp_c")]
        public float TemperatureC { get; set; }

        /// <summary>
        /// Fahrenhayt cinsinden sıcaklık
        /// </summary>
        [DataMember]
        [JsonProperty("temp_f")]
        public float TemperatureF { get; set; }

        /// <summary>
        /// Gündüz mü gece mi? 1 ise gündüz, 0 ise gece
        /// </summary>
        [DataMember]
        [JsonProperty("is_day")]
        public int IsDay { get; set; }

        /// <summary>
        /// Hava Durumu
        /// </summary>
        [DataMember]
        [JsonProperty("condition")]
        public WeatherCondition Condition { get; set; }

        /// <summary>
        /// Saatte mil cinsinden rüzgar hızı
        /// </summary>
        [DataMember]
        [JsonProperty("wind_mph")]
        public float WindSpeedMph { get; set; }

        /// <summary>
        /// Saatte kilometre cinsinden rüzgar hızı
        /// </summary>
        [DataMember]
        [JsonProperty("wind_kph")]
        public float WindSpeedKph { get; set; }

        /// <summary>
        /// Derece cinsinden rüzgar yönü
        /// </summary>
        [DataMember]
        [JsonProperty("wind_degree")]
        public int WindDegree { get; set; }

        /// <summary>
        /// 16 noktalı pusula olarak rüzgar yönü. ör.: NSW
        /// </summary>
        [DataMember]
        [JsonProperty("wind_dir")]
        public string WindDir { get; set; }

        /// <summary>
        /// Milibar cinsinden basınç
        /// </summary>
        [DataMember]
        [JsonProperty("pressure_mb")]
        public float PressureMb { get; set; }

        /// <summary>
        /// inç cinsinden basınç
        /// </summary>
        [DataMember]
        [JsonProperty("pressure_in")]
        public float PressureIn { get; set; }

        /// <summary>
        /// Milimetre cinsinden yağış miktarı
        /// </summary>
        [DataMember]
        [JsonProperty("precip_mm")]
        public float PrecipMM { get; set; }

        /// <summary>
        /// İnç cinsinden yağış miktarı
        /// </summary>
        [DataMember]
        [JsonProperty("precip_in")]
        public float PrecipIn { get; set; }

        /// <summary>
        /// Yüzde olarak nem
        /// </summary>
        [DataMember]
        [JsonProperty("humidity")]
        public int Humidity { get; set; }

        /// <summary>
        /// Yüzde olarak bulut örtüsü
        /// </summary>
        [DataMember]
        [JsonProperty("cloud")]
        public int Cloud { get; set; }

        /// <summary>
        /// Santigratta sıcaklık hissedilen
        /// </summary>
        [DataMember]
        [JsonProperty("feelslike_c")]
        public float FeelslikeC { get; set; }

        /// <summary>
        /// Fahrenhayt cinsinden sıcaklık hissedilen
        /// </summary>
        [DataMember]
        [JsonProperty("feelslike_f")]
        public float FeelslikeF { get; set; }

        /// <summary>
        /// Kilometre olarak görünürlük
        /// </summary>
        [DataMember]
        [JsonProperty("vis_km")]
        public float VisKM { get; set; }

        /// <summary>
        /// Mil olarak görünürlük
        /// </summary>
        [DataMember]
        [JsonProperty("vis_miles")]
        public float VisMiles { get; set; }

        /// <summary>
        /// UV Endeksi
        /// </summary>
        [DataMember]
        [JsonProperty("uv")]
        public float UV { get; set; }

        /// <summary>
        /// Saatte mil cinsinden esen rüzgar
        /// </summary>
        [DataMember]
        [JsonProperty("gust_mph")]
        public float GustMph { get; set; }

        /// <summary>
        /// Saatte kilometre cinsinden esen rüzgar
        /// </summary>
        [DataMember]
        [JsonProperty("gust_kph")]
        public float GustKph { get; set; }
    }

    public class WeatherCondition
    {
        /// <summary>
        /// Metin
        /// </summary>
        [DataMember]
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>
        /// ikon url
        /// </summary>
        [DataMember]
        [JsonProperty("icon")]
        public string IconUrl { get; set; }

        /// <summary>
        /// benzersiz kod
        /// </summary>
        [DataMember]
        [JsonProperty("code")]
        public int Code { get; set; }
    }
}
