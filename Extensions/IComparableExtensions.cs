using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia
{
    public static class IComparableExtensions
    {
        public static bool Between<T>(this T value, T from, T to) where T : IComparable<T>
        {
            return value.CompareTo(from) >= 0 && value.CompareTo(to) <= 0;
        }
    }
}
