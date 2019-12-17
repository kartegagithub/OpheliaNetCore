using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia
{
    public static class BooleanExtensions
    {
        public static string ToYesNo(this bool value)
        {
            return value ? "Yes" : "No";
        }
    }
}
