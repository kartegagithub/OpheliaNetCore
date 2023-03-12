using System;
using System.Globalization;

namespace Ophelia
{
    public static class DoubleExtensions
    {
        /// <summary>
        /// Double değeri string'e döndürür.
        /// </summary>
        public static string ToStringInvariant(this double value)
        {
            return value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Ondalıklı sayıyı string veri tipine çevirir.
        /// </summary>
        public static string ToPointString(this double? point)
        {
            if (point.HasValue)
            {
                var numberFormat = new NumberFormatInfo();
                numberFormat.NumberDecimalSeparator = ".";
                return Convert.ToString(point.Value, numberFormat);
            }
            return string.Empty;
        }
    }
}
