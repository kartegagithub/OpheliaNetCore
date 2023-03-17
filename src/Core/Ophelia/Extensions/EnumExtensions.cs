using System;
using System.Collections.Generic;
using System.Linq;
using Ophelia;

namespace Ophelia
{
    public static class EnumExtensions
    {
        public static int ToInt32(this Enum value)
        {
            return Convert.ToInt32(value);
        }

        public static byte ToByte(this Enum value)
        {
            return Convert.ToByte(value);
        }

        public static string GetValue<T>(this Enum value)
        {
            return Convert.ChangeType(value, typeof(T)).ToString();
        }
        public static List<TEnum> ToList<TEnum>()
        {
            Type type = typeof(TEnum);

            if (type.BaseType == typeof(Enum))
                throw new ArgumentException("T must be type of System.Enum");

            Array values = Enum.GetValues(type);
            if (values.Length > 0)
                return values.Cast<TEnum>().ToList();
            return null;
        }
    }
}
