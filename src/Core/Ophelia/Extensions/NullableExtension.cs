using System;
using System.Collections.Generic;
using System.Linq;

namespace Ophelia
{
    public static class NullableExtension
    {
        public static Nullable<T> AsNullable<T>(this T value) where T : struct
        {
            return new Nullable<T>(value);
        }
        public static List<long?> ToNullable(this List<long> value)
        {
            return (from item in value select new Nullable<long>(item)).ToList();
        }
        public static List<int?> ToNullable(this List<int> value)
        {
            return (from item in value select new Nullable<int>(item)).ToList();
        }
        public static List<decimal?> ToNullable(this List<decimal> value)
        {
            return (from item in value select new Nullable<decimal>(item)).ToList();
        }
        public static List<double?> ToNullable(this List<double> value)
        {
            return (from item in value select new Nullable<double>(item)).ToList();
        }
    }
}
