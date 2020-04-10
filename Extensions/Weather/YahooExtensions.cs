using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Ophelia
{
    public static class YahooExtensions
    {
        public static string GetWeather(string appID, string consumerKey, string consumerSecret, string location, string format = "json")
        {
            string cURL = "https://weather-ydn-yql.media.yahoo.com/forecastrss";
            string cAppID = appID;
            string cConsumerKey = consumerKey;
            string cConsumerSecret = consumerSecret;
            string cOAuthVersion = "1.0";
            string cOAuthSignMethod = "HMAC-SHA1";
            string cWeatherID = location;
            string cUnitID = "u=c";
            string cFormat = format;

            string _get_timestamp()
            {
                TimeSpan lTS = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                return Convert.ToInt64(lTS.TotalSeconds).ToString();
            }

            string _get_nonce()
            {
                return Convert.ToBase64String(
                 new ASCIIEncoding().GetBytes(
                  DateTime.Now.Ticks.ToString()
                 )
                );
            }


            string _get_auth()
            {
                string retVal;
                string lNonce = _get_nonce();
                string lTimes = _get_timestamp();
                string lCKey = string.Concat(cConsumerSecret, "&");
                string lSign = string.Format(
                 "format={0}&" +
                 "location={1}&" +
                 "oauth_consumer_key={2}&" +
                 "oauth_nonce={3}&" +
                 "oauth_signature_method={4}&" +
                 "oauth_timestamp={5}&" +
                 "oauth_version={6}&" +
                 "{7}",
                 cFormat,
                 cWeatherID,
                 cConsumerKey,
                 lNonce,
                 cOAuthSignMethod,
                 lTimes,
                 cOAuthVersion,
                 cUnitID
                );

                lSign = string.Concat(
                 "GET&", Uri.EscapeDataString(cURL), "&", Uri.EscapeDataString(lSign)
                );

                using (var lHasher = new HMACSHA1(Encoding.ASCII.GetBytes(lCKey)))
                {
                    lSign = Convert.ToBase64String(
                     lHasher.ComputeHash(Encoding.ASCII.GetBytes(lSign))
                    );
                }  // end using

                return "OAuth " +
                       "oauth_consumer_key=\"" + cConsumerKey + "\", " +
                       "oauth_nonce=\"" + lNonce + "\", " +
                       "oauth_timestamp=\"" + lTimes + "\", " +
                       "oauth_signature_method=\"" + cOAuthSignMethod + "\", " +
                       "oauth_signature=\"" + lSign + "\", " +
                       "oauth_version=\"" + cOAuthVersion + "\"";

            }  // end _get_auth


            string url = cURL + "?location=" + cWeatherID + "&" + cUnitID + "&format=" + cFormat;
            using (var client = new WebClient())
            {
                string responseText = string.Empty;
                try
                {
                    string headerString = _get_auth();

                    WebClient webClient = new WebClient();
                    webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
                    webClient.Headers[HttpRequestHeader.Authorization] = headerString;
                    webClient.Headers.Add("X-Yahoo-App-Id", cAppID);
                    byte[] reponse = webClient.DownloadData(url);
                    string lOut = Encoding.ASCII.GetString(reponse);
                    return lOut;
                }
                catch (WebException exception)
                {
                    if (exception.Response != null)
                    {
                        var responseStream = exception.Response.GetResponseStream();
                        if (responseStream != null)
                        {
                            using (var reader = new StreamReader(responseStream))
                            {
                                responseText = reader.ReadToEnd();

                            }
                        }
                    }

                }
            }
            return "";
        }
    }
}