using System;
using System.Globalization;

namespace Ophelia
{
    public static class DecimalExtensions
    {
        /// <summary>
        /// Ondalıklı değeri stringe çevirir.
        /// </summary>
        public static string ToStringInvariant(this decimal value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Ondalıklı sayının tam sayı kısmını alır.
        /// </summary>
        public static string ToStringIntegral(this decimal value)
        {
            var left = Math.Floor(value);
            return string.Format("{0:0}", left);
        }

        /// <summary>
        /// Ondalıklı sayının küsürat kısmını alır.
        /// </summary>
        public static string ToStringFraction(this decimal value)
        {
            var left = Math.Floor(value);
            var right = value - left;
            return string.Format("{0:.##}", right);
        }

        public static string ToRoundedPriceString(this decimal value, int partOfString = -1)
        {
            var roundedValue = Math.Round(value, 2, MidpointRounding.AwayFromZero);
            string returnValue = string.Format("{0:.00}", roundedValue, CultureInfo.GetCultureInfo("tr-TR"));
            if (partOfString > -1 && partOfString < 2)
                returnValue = returnValue.Split(new char[] { ',', '.' })[partOfString];
            if (string.IsNullOrEmpty(returnValue.Trim())) returnValue = "0";
            return returnValue;
        }

        /// <summary>
        /// Yüzdelik oranını hesaplar.
        /// </summary>
        public static decimal Percent(this decimal baseValue, decimal value)
        {
            if (value == 0)
                return 0;

            return Math.Floor((baseValue - value) / value * 100);
        }

        /// <summary>
        /// Ondalıklı sayıyı string veri tipine çevirir.
        /// </summary>
        public static string ToPointString(this decimal? point)
        {
            if (point.HasValue)
            {
                var numberFormat = new NumberFormatInfo();
                numberFormat.NumberDecimalSeparator = ".";
                return Convert.ToString(point.Value, numberFormat);
            }
            return string.Empty;
        }

        public static string ToFormattedString(this decimal? value)
        {
            if (value.HasValue)
                return value.Value.ToFormattedString();
            return string.Empty;
        }

        public static string ToFormattedString(this decimal value)
        {
            return value.ToString("N2", CultureInfo.InvariantCulture);
        }
    }
}
