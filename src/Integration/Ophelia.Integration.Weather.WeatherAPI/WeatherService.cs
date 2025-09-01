using Ophelia.Integration.Weather.WeatherAPI.Model;
using Ophelia.Service;
using System;

namespace Ophelia.Integration.Weather.WeatherAPI
{
    /// <summary>
    /// Kullanılan Site: https://www.weatherapi.com/docs/
    /// </summary>
    public class WeatherService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ServiceURL"></param>
        /// <param name="APIKey"></param>
        public WeatherService(string ServiceURL, string APIKey)
        {
            this.ServiceURL = ServiceURL;
            this.APIKey = APIKey;
        }

        /// <summary>
        /// Servis Linki
        /// </summary>
        public string ServiceURL { get; set; }

        /// <summary>
        /// Serviste kullanılacak API Key
        /// </summary>
        public string APIKey { get; set; }

        /// <summary>
        /// Girilen bilgilere göre anlık hava durumu bilgisi döner. İl (İlçe) veya enlem & boylam girmek zorunludur.
        /// </summary>
        /// <param name="city">İl/İlçe</param>
        /// <param name="lang">Dil. Örn: tr Varsayılan olarak İngilizcedir</param>
        /// <param name="latitude">Enlem</param>
        /// <param name="longitude">Boylam</param>
        public ServiceObjectResult<WeatherResult> CurrentWeather(string city, string lang, string latitude, string longitude)
        {
            var result = new ServiceObjectResult<WeatherResult>();
            try
            {
                if (!string.IsNullOrEmpty(lang))
                    lang = $"&lang={lang}";

                var q = "";
                if (!string.IsNullOrEmpty(city))
                {
                    q = $"&q={city}";
                }
                else if (!string.IsNullOrEmpty(latitude) && !string.IsNullOrEmpty(longitude))
                {
                    q = $"&q={latitude},{longitude}";
                }
                else
                {
                    result.Fail("CityOrCoordinateCanNotBeBlank");
                    return result;
                }

                var URL = $"{this.ServiceURL}?key={this.APIKey}{q}{lang}";

                var serviceResult = URL.DownloadURL();
                if (!string.IsNullOrEmpty(serviceResult))
                    result.SetData(serviceResult.FromJson<WeatherResult>());
                else
                    result.Fail("AProblemOccurred");
            }
            catch (Exception ex)
            {
                result.Fail(ex);
            }
            return result;
        }
    }
}