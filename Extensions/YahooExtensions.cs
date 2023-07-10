using Newtonsoft.Json;
using Ophelia.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                return DateTime.UtcNow.ConvertToJSDate().ToString();
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

                    using (WebClient webClient = new WebClient())
                    {
                        webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
                        webClient.Headers[HttpRequestHeader.Authorization] = headerString;
                        webClient.Headers.Add("X-Yahoo-App-Id", cAppID);
                        byte[] reponse = webClient.DownloadData(url);
                        string lOut = Encoding.ASCII.GetString(reponse);
                        return lOut;
                    }
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

        /// <summary>
        /// Hisse senedinin chart bilgilerini döner
        /// </summary>
        /// <param name="stockCode">hisse senedi kodu</param>
        /// <param name="region">bölge kodu</param>
        /// <param name="lang">dil culture kodu</param>
        /// <param name="interval">ne kadar aralık ile çekileceği --1m, 2m, 5m, 15m, 30m, 60m, 90m, 1h, 1d, 5d, 1wk, 1mo, 3mo</param>
        /// <param name="range">ne kadar uzunlukta çekileceği --1d, 5d, 1mo, 3mo, 6mo, 1y, 2y, 5y, 10y, ytd, max</param>
        /// <returns></returns>
        public static StockChartResult GetStockChart(this string stockCode, string region = "US", string lang = "en-US", string interval = "2m", string range = "1d")
        {
            StockChartResult result = null;
            var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{stockCode}?region={region}&lang={lang}&includePrePost=false&interval={interval}&useYfid=true&range={range}&corsDomain=finance.yahoo.com&.tsrc=finance";

            var response = url.DownloadURL();
            if (!string.IsNullOrEmpty(response))
            {
                result = response.FromJson<StockChartResult>();
            }

            return result;
        }

        /// <summary>
        /// Hisse senedinin detay bilgilerini döner
        /// </summary>
        /// <param name="stockCode">hisse senedi kodu</param>
        /// <returns></returns>
        public static StockInformationResult GetStockInformation(this string stockCode)
        {
            StockInformationResult result = null;
            var url = $"https://query2.finance.yahoo.com/v7/finance/options/{stockCode}";

            var response = url.DownloadURL();
            var asd = response.Contains("quote");
            if (!string.IsNullOrEmpty(response))
            {
                result = response.FromJson<StockInformationResult>();
            }

            return result;
        }

        /// <summary>
        /// Hisse senedinin detay bilgilerini döner
        /// </summary>
        /// <param name="stockCode">hisse senedi kodu</param>
        /// <param name="region">bölge kodu</param>
        /// <param name="lang">dil culture kodu</param>
        /// <param name="interval">ne kadar aralık ile çekileceği --1m, 2m, 5m, 15m, 30m, 60m, 90m, 1h, 1d, 5d, 1wk, 1mo, 3mo</param>
        /// <param name="range">ne kadar uzunlukta çekileceği --1d, 5d, 1mo, 3mo, 6mo, 1y, 2y, 5y, 10y, ytd, max</param>
        /// <returns></returns>
        public static StockModel GetStockDetail(this string stockCode, string region = "US", string lang = "en-US", string interval = "2m", string range = "1d")
        {
            var result = new StockModel
            {
                Histories = new List<StockHistory>(),
                Ranges = GetStockChartRanges()
            };

            if (!string.IsNullOrEmpty(stockCode))
            {
                var stockDetailResult = stockCode.GetStockInformation();
                if (stockDetailResult != null && stockDetailResult.QuoteResponse != null && stockDetailResult.QuoteResponse.Result?.Count > 0)
                {
                    var detailResult = stockDetailResult.QuoteResponse.Result.FirstOrDefault();
                    result.LastPrice = decimal.Round(detailResult.quote.RegularMarketPrice, 2);
                    result.Date = detailResult.quote.FirstTradeDateMilliseconds.ConvertFromJSDate();
                    result.PriceChange = decimal.Round(detailResult.quote.RegularMarketChangePercent, 2);
                }

                var stockChartResult = stockCode.GetStockChart(region, lang, interval, range);
                if (stockChartResult != null && stockChartResult.Chart != null && stockChartResult.Chart.Result?.Count > 0)
                {
                    var chartResult = stockChartResult.Chart.Result.FirstOrDefault();
                    if (chartResult.Indicators != null && chartResult.Indicators.Quote?.Count > 0 && chartResult.Timestamp?.Count > 0)
                    {
                        var quote = chartResult.Indicators.Quote.FirstOrDefault();
                        if (quote.Close?.Count > 0)
                        {
                            var startDate = DateTime.Now;
                            switch (range)
                            {
                                case "1d":
                                    startDate = DateTime.Now.AddDays(-1);
                                    break;
                                case "5d":
                                    startDate = DateTime.Now.AddDays(-5);
                                    break;
                                case "1mo":
                                    startDate = DateTime.Now.AddMonths(-1);
                                    break;
                                case "6mo":
                                    startDate = DateTime.Now.AddMonths(-6);
                                    break;
                                case "ytd":
                                    startDate = DateTime.Now.StartOfYear();
                                    break;
                                case "1y":
                                    startDate = DateTime.Now.AddYears(-1);
                                    break;
                                case "5y":
                                    startDate = DateTime.Now.AddYears(-5).AddDays(1);
                                    break;
                                case "max":
                                    startDate = result.Date;
                                    break;
                                default:
                                    break;
                            }

                            for (int i = 0; i < quote.Close.Count; i++)
                            {
                                var item = quote.Close[i];
                                if (!string.IsNullOrEmpty(item))
                                {
                                    if (i == 0)
                                    {
                                        var time = chartResult.Timestamp[i];
                                        var date = time.ConvertFromJSDate();

                                        startDate = DateTime.ParseExact(startDate.ToString("dd.MM.yyyy") + " " + date.ToString("HH:mm:ss"), "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                                    }

                                    var value = item.ToDecimal();
                                    result.Histories.Add(new StockHistory() { Date = startDate, Value = decimal.Round(value, 2) });
                                }

                                switch (interval)
                                {
                                    case "2m":
                                        startDate = startDate.AddMinutes(2);
                                        break;
                                    case "15m":
                                        startDate = startDate.AddMinutes(15);
                                        break;
                                    case "30m":
                                        startDate = startDate.AddMinutes(30);
                                        break;
                                    case "1d":
                                        startDate = startDate.AddDays(1);
                                        break;
                                    case "1wk":
                                        startDate = startDate.AddDays(7);
                                        break;
                                    case "1mo":
                                        startDate = startDate.AddMonths(1);
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Hisse senedinin aralık bilgilerini döner
        /// </summary>
        /// <returns></returns>
        public static List<StockRange> GetStockChartRanges()
        {
            var list = new List<StockRange>
            {
                new StockRange { Key = "2m", Value = "1d" },
                new StockRange { Key = "15m", Value = "5d" },
                new StockRange { Key = "30m", Value = "1mo" },
                new StockRange { Key = "1d", Value = "6mo" },
                new StockRange { Key = "1d", Value = "ytd" },
                new StockRange { Key = "1d", Value = "1y" },
                new StockRange { Key = "1wk", Value = "5y" },
                new StockRange { Key = "1mo", Value = "max" }
            };
            return list;
        }
    }
}