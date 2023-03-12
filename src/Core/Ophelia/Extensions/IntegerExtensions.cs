using System;
using System.Globalization;

namespace Ophelia
{
    public static class IntegerExtensions
    {
        public static int Half(this int source)
        {
            return source / 2;
        }

        public static int Cube(this int source)
        {
            return (int)Math.Pow(source, 3);
        }

        public static int Square(this int source)
        {
            return (int)Math.Pow(source, 2);
        }

        public static bool IsEvenNumber(this int number)
        {
            return ((number % 2) == 0);
        }

        public static bool IsOddNumber(this int number)
        {
            return ((number % 2) == 1);
        }

        public static string ToMonthString(this int value)
        {
            return (value >= 1 && value <= 12) ? CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(value) : "";
        }

        public static decimal ToPixel(this int point)
        {
            return Decimal.Truncate((point * 4) / 5) + 1;
        }

        public static int Seconds(this int source)
        {
            return source * 1000;
        }
        public static string ToSizeString(this int val)
        {
            return Convert.ToInt64(val).ToSizeString();
        }
        public static string ToSizeString(this long val)
        {
            if (val < 1024)
                return val + "B";
            else if (val < Math.Pow(1024, 2))
                return Math.Round(Convert.ToDecimal(val / 1024), 2).ToString("N2") + "KB";
            else if (val < Math.Pow(1024, 3))
                return Math.Round(Convert.ToDecimal(val / (1024 * 1024)), 2).ToString("N2") + "MB";
            else if (val < Math.Pow(1024, 4))
                return Math.Round(Convert.ToDecimal(val / (1024 * 1024 * 1024)), 2).ToString("N2") + "GB";

            return val.ToString();
        }
    }
}
