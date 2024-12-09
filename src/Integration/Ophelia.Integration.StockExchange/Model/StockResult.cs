using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization;

namespace Ophelia.Integration.StockExchange.Model
{
	[DataContract(IsReference = true)]
	public class StockChartResult
	{
		[DataMember]
		[JsonProperty("chart")]
		public StockChart Chart { get; set; }
	}

	[DataContract(IsReference = true)]
	public class StockChart
	{
		[DataMember]
		[JsonProperty("result")]
		public List<ChartResult> Result { get; set; }

		[DataMember]
		[JsonProperty("error")]
		public string Error { get; set; }
	}

	[DataContract(IsReference = true)]
	public class ChartResult
	{
		[DataMember]
		[JsonProperty("meta")]
		public StockMetaData Meta { get; set; }

		[DataMember]
		[JsonProperty("timestamp")]
		public List<long> Timestamp { get; set; }

		[DataMember]
		[JsonProperty("indicators")]
		public StockIndicator Indicators { get; set; }
	}

	[DataContract(IsReference = true)]
	public class StockMetaData
	{
		[DataMember]
		[JsonProperty("currency")]
		public string Currency { get; set; }

		[DataMember]
		[JsonProperty("symbol")]
		public string Symbol { get; set; }

		[DataMember]
		[JsonProperty("exchangeName")]
		public string ExchangeName { get; set; }

		[DataMember]
		[JsonProperty("instrumentType")]
		public string InstrumentType { get; set; }

		[DataMember]
		[JsonProperty("firstTradeDate")]
		public long FirstTradeDate { get; set; }

		[DataMember]
		[JsonProperty("regularMarketTime")]
		public long RegularMarketTime { get; set; }

		[DataMember]
		[JsonProperty("gmtoffset")]
		public long Gmtoffset { get; set; }

		[DataMember]
		[JsonProperty("timezone")]
		public string Timezone { get; set; }

		[DataMember]
		[JsonProperty("exchangeTimezoneName")]
		public string ExchangeTimezoneName { get; set; }

		[DataMember]
		[JsonProperty("regularMarketPrice")]
		public decimal RegularMarketPrice { get; set; }

		[DataMember]
		[JsonProperty("chartPreviousClose")]
		public decimal ChartPreviousClose { get; set; }

		[DataMember]
		[JsonProperty("previousClose")]
		public decimal PreviousClose { get; set; }

		[DataMember]
		[JsonProperty("scale")]
		public long Scale { get; set; }

		[DataMember]
		[JsonProperty("priceHint")]
		public long PriceHint { get; set; }

		[DataMember]
		[JsonProperty("currentTradingPeriod")]
		public StockCurrentTradingPeriod CurrentTradingPeriod { get; set; }

		[DataMember]
		[JsonProperty("tradingPeriods")]
		public List<List<StockTradingPeriod>> TradingPeriods { get; set; }

		[DataMember]
		[JsonProperty("dataGranularity")]
		public string DataGranularity { get; set; }

		[DataMember]
		[JsonProperty("range")]
		public string Range { get; set; }

		[DataMember]
		[JsonProperty("validRanges")]
		public List<string> ValidRanges { get; set; }
	}

	[DataContract(IsReference = true)]
	public class StockCurrentTradingPeriod
	{
		[DataMember]
		[JsonProperty("pre")]
		public StockTradingPeriod Pre { get; set; }

		[DataMember]
		[JsonProperty("regular")]
		public StockTradingPeriod Regular { get; set; }

		[DataMember]
		[JsonProperty("post")]
		public StockTradingPeriod Post { get; set; }
	}

	[DataContract(IsReference = true)]
	public class StockTradingPeriod
	{
		[DataMember]
		[JsonProperty("timezone")]
		public string Timezone { get; set; }

		[DataMember]
		[JsonProperty("start")]
		public long Start { get; set; }

		[DataMember]
		[JsonProperty("end")]
		public long End { get; set; }

		[DataMember]
		[JsonProperty("gmtoffset")]
		public long Gmtoffset { get; set; }
	}

	[DataContract(IsReference = true)]
	public class StockIndicator
	{
		[DataMember]
		[JsonProperty("quote")]
		public List<StockQuote> Quote { get; set; }
	}

	[DataContract(IsReference = true)]
	public class StockQuote
	{
		[DataMember]
		[JsonProperty("open")]
		public List<string> Open { get; set; }

		[DataMember]
		[JsonProperty("volume")]
		public List<string> Volume { get; set; }

		[DataMember]
		[JsonProperty("low")]
		public List<string> Low { get; set; }

		[DataMember]
		[JsonProperty("high")]
		public List<string> High { get; set; }

		[DataMember]
		[JsonProperty("close")]
		public List<string> Close { get; set; }
	}

	[DataContract(IsReference = true)]
	public class StockRange
	{
		[DataMember]
		public string Key { get; set; }

		[DataMember]
		public string Value { get; set; }
	}

	[DataContract(IsReference = true)]
	public class StockInformationResult
	{
		[DataMember]
		[JsonProperty("quoteResponse")]
		public StockInformation QuoteResponse { get; set; }
	}

	[DataContract(IsReference = true)]
	public class StockInformation
	{
		[DataMember]
		[JsonProperty("result")]
		public List<StockInformationDetail> Result { get; set; }

		[DataMember]
		[JsonProperty("error")]
		public string Error { get; set; }
	}

	[DataContract(IsReference = true)]
	public class StockInformationResultModel
	{
		[DataMember]
		[JsonProperty("quote")]
		public StockInformationDetail Quote { get; set; }
	}

	[DataContract(IsReference = true)]
	public class StockInformationDetail
	{
		[DataMember]
		[JsonProperty("language")]
		public string Language { get; set; }

		[DataMember]
		[JsonProperty("region")]
		public string Region { get; set; }

		[DataMember]
		[JsonProperty("quoteType")]
		public string QuoteType { get; set; }

		[DataMember]
		[JsonProperty("triggerable")]
		public bool Triggerable { get; set; }

		[DataMember]
		[JsonProperty("currency")]
		public string Currency { get; set; }

		[DataMember]
		[JsonProperty("regularMarketVolume")]
		public decimal RegularMarketVolume { get; set; }

		[DataMember]
		[JsonProperty("bid")]
		public decimal Bid { get; set; }

		[DataMember]
		[JsonProperty("shortName")]
		public string ShortName { get; set; }

		[DataMember]
		[JsonProperty("marketState")]
		public string MarketState { get; set; }

		[DataMember]
		[JsonProperty("exchange")]
		public string Exchange { get; set; }

		[DataMember]
		[JsonProperty("longName")]
		public string LongName { get; set; }

		[DataMember]
		[JsonProperty("messageBoardId")]
		public string MessageBoardId { get; set; }

		[DataMember]
		[JsonProperty("exchangeTimezoneName")]
		public string ExchangeTimezoneName { get; set; }

		[DataMember]
		[JsonProperty("exchangeTimezoneShortName")]
		public string ExchangeTimezoneShortName { get; set; }

		[DataMember]
		[JsonProperty("gmtOffSetMilliseconds")]
		public long GmtOffSetMilliseconds { get; set; }

		[DataMember]
		[JsonProperty("esgPopulated")]
		public bool EsgPopulated { get; set; }

		[DataMember]
		[JsonProperty("firstTradeDateMilliseconds")]
		public long FirstTradeDateMilliseconds { get; set; }

		[DataMember]
		[JsonProperty("priceHint")]
		public int PriceHint { get; set; }

		[DataMember]
		[JsonProperty("regularMarketChange")]
		public decimal RegularMarketChange { get; set; }

		[DataMember]
		[JsonProperty("regularMarketChangePercent")]
		public decimal RegularMarketChangePercent { get; set; }

		[DataMember]
		[JsonProperty("regularMarketTime")]
		public long RegularMarketTime { get; set; }

		[DataMember]
		[JsonProperty("regularMarketPrice")]
		public decimal RegularMarketPrice { get; set; }

		[DataMember]
		[JsonProperty("regularMarketDayHigh")]
		public decimal RegularMarketDayHigh { get; set; }

		[DataMember]
		[JsonProperty("regularMarketDayRange")]
		public string RegularMarketDayRange { get; set; }

		[DataMember]
		[JsonProperty("regularMarketDayLow")]
		public decimal RegularMarketDayLow { get; set; }

		[DataMember]
		[JsonProperty("regularMarketPreviousClose")]
		public decimal RegularMarketPreviousClose { get; set; }

		[DataMember]
		[JsonProperty("ask")]
		public decimal Ask { get; set; }

		[DataMember]
		[JsonProperty("bidSize")]
		public int BidSize { get; set; }

		[DataMember]
		[JsonProperty("askSize")]
		public int AskSize { get; set; }

		[DataMember]
		[JsonProperty("fullExchangeName")]
		public string FullExchangeName { get; set; }

		[DataMember]
		[JsonProperty("financialCurrency")]
		public string FinancialCurrency { get; set; }

		[DataMember]
		[JsonProperty("regularMarketOpen")]
		public decimal RegularMarketOpen { get; set; }

		[DataMember]
		[JsonProperty("averageDailyVolume3Month")]
		public long AverageDailyVolume3Month { get; set; }

		[DataMember]
		[JsonProperty("averageDailyVolume10Day")]
		public long AverageDailyVolume10Day { get; set; }

		[DataMember]
		[JsonProperty("fiftyTwoWeekLowChange")]
		public decimal FiftyTwoWeekLowChange { get; set; }

		[DataMember]
		[JsonProperty("fiftyTwoWeekLowChangePercent")]
		public decimal FiftyTwoWeekLowChangePercent { get; set; }

		[DataMember]
		[JsonProperty("fiftyTwoWeekRange")]
		public string FiftyTwoWeekRange { get; set; }

		[DataMember]
		[JsonProperty("fiftyTwoWeekHighChange")]
		public decimal fiftyTwoWeekHighChange { get; set; }

		[DataMember]
		[JsonProperty("fiftyTwoWeekHighChangePercent")]
		public decimal FiftyTwoWeekHighChangePercent { get; set; }

		[DataMember]
		[JsonProperty("fiftyTwoWeekLow")]
		public decimal FiftyTwoWeekLow { get; set; }

		[DataMember]
		[JsonProperty("fiftyTwoWeekHigh")]
		public decimal FiftyTwoWeekHigh { get; set; }

		[DataMember]
		[JsonProperty("earningsTimestamp")]
		public long EarningsTimestamp { get; set; }

		[DataMember]
		[JsonProperty("earningsTimestampStart")]
		public long EarningsTimestampStart { get; set; }

		[DataMember]
		[JsonProperty("earningsTimestampEnd")]
		public long EarningsTimestampEnd { get; set; }

		[DataMember]
		[JsonProperty("trailingPE")]
		public decimal TrailingPE { get; set; }

		[DataMember]
		[JsonProperty("epsTrailingTwelveMonths")]
		public decimal EpsTrailingTwelveMonths { get; set; }

		[DataMember]
		[JsonProperty("sharesOutstanding")]
		public long SharesOutstanding { get; set; }

		[DataMember]
		[JsonProperty("bookValue")]
		public decimal BookValue { get; set; }

		[DataMember]
		[JsonProperty("fiftyDayAverage")]
		public decimal FiftyDayAverage { get; set; }

		[DataMember]
		[JsonProperty("fiftyDayAverageChange")]
		public decimal FiftyDayAverageChange { get; set; }

		[DataMember]
		[JsonProperty("fiftyDayAverageChangePercent")]
		public decimal FiftyDayAverageChangePercent { get; set; }

		[DataMember]
		[JsonProperty("twoHundredDayAverage")]
		public decimal TwoHundredDayAverage { get; set; }

		[DataMember]
		[JsonProperty("twoHundredDayAverageChange")]
		public decimal TwoHundredDayAverageChange { get; set; }

		[DataMember]
		[JsonProperty("twoHundredDayAverageChangePercent")]
		public decimal TwoHundredDayAverageChangePercent { get; set; }

		[DataMember]
		[JsonProperty("marketCap")]
		public long MarketCap { get; set; }

		[DataMember]
		[JsonProperty("priceToBook")]
		public decimal PriceToBook { get; set; }

		[DataMember]
		[JsonProperty("sourceInterval")]
		public int SourceInterval { get; set; }

		[DataMember]
		[JsonProperty("exchangeDataDelayedBy")]
		public int ExchangeDataDelayedBy { get; set; }

		[DataMember]
		[JsonProperty("tradeable")]
		public bool Tradeable { get; set; }

		[DataMember]
		[JsonProperty("symbol")]
		public string Symbol { get; set; }
	}

	[DataContract(IsReference = true)]
	public class StockHistory
	{
		[DataMember]
		public DateTime Date { get; set; }

		[DataMember]
		public decimal Value { get; set; }
	}

	[DataContract(IsReference = true)]
	public class StockModel
	{
		[DataMember]
		public string Code { get; set; }

		[DataMember]
		public DateTime Date { get; set; }

		[DataMember]
		public decimal LastPrice { get; set; }

		[DataMember]
		public decimal PriceChange { get; set; }

		[DataMember]
		public List<StockHistory> Histories { get; set; }

		[DataMember]
		public List<StockRange> Ranges { get; set; }
	}
}
