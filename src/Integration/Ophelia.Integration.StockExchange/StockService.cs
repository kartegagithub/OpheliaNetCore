using Ophelia.Integration.StockExchange.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace Ophelia.Integration.StockExchange
{
    /// <summary>
    /// Kullanılan Site: https://www.weatherapi.com/docs/
    /// </summary>
    public static class StockService
    {
        private static string sCookie { get; set; }
        private static string Cookie
        {
            get
            {
                if (string.IsNullOrEmpty(sCookie))
                {
                    using (var client = new HttpClient())
                    {
                        var responasdse = client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://fc.yahoo.com/"));

                        if (responasdse.Result.Headers.TryGetValues("Set-Cookie", out var cookies))
                        {
                            foreach (var item in cookies)
                            {
                                sCookie += item + ";";

                            }
                        }
                    }
                }
                return sCookie;
            }
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
            var crumb = GetCrumb();

            var url = $"https://query2.finance.yahoo.com/v7/finance/quote?symbols={stockCode}&crumb={crumb}";

            WebHeaderCollection headers = new WebHeaderCollection
            {
                { "Cookie", Cookie }
            };

            var response = url.DownloadURL(headers: headers);
            if (!string.IsNullOrEmpty(response))
            {
                result = response.FromJson<StockInformationResult>();
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        private static string GetCrumb()
        {
            WebHeaderCollection headers = new WebHeaderCollection
            {
                { "Cookie", Cookie }
            };

            string result = "Yb2ckR7FNfs";
            var url = $"https://query2.finance.yahoo.com/v1/test/getcrumb";

            var response = url.DownloadURL(headers: headers);
            if (!string.IsNullOrEmpty(response) && !response.Contains("Unauthorized"))
            {
                result = response;
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
                    result.LastPrice = decimal.Round(detailResult.RegularMarketPrice, 2);
                    result.Date = detailResult.FirstTradeDateMilliseconds.ConvertFromJSDate();
                    result.PriceChange = decimal.Round(detailResult.RegularMarketChangePercent, 2);
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
                            var startDate = Utility.Now;
                            switch (range)
                            {
                                case "1d":
                                    startDate = Utility.Now.AddDays(-1);
                                    break;
                                case "5d":
                                    startDate = Utility.Now.AddDays(-5);
                                    break;
                                case "1mo":
                                    startDate = Utility.Now.AddMonths(-1);
                                    break;
                                case "6mo":
                                    startDate = Utility.Now.AddMonths(-6);
                                    break;
                                case "ytd":
                                    startDate = Utility.Now.StartOfYear();
                                    break;
                                case "1y":
                                    startDate = Utility.Now.AddYears(-1);
                                    break;
                                case "5y":
                                    startDate = Utility.Now.AddYears(-5).AddDays(1);
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