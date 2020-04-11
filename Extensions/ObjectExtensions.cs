using System;
using System.Collections.Generic;
using System.Text;

namespace Ophelia
{
    public static class ObjectExtensions
    {
        public static Int16 ToInt16(this object value)
        {
            Int16 result = 0;

            if (value != null && value.ToString().IsNumeric())
                Int16.TryParse(value.ToString(), out result);

            return result;
        }

        public static Int32 ToInt32(this object value)
        {
            Int32 result = 0;

            if (value != null && value.ToString().IsNumeric())
                Int32.TryParse(value.ToString(), out result);

            return result;
        }

        public static long ToInt64(this object value)
        {
            Int64 result = 0;

            if (value != null && value.ToString().IsNumeric())
                Int64.TryParse(value.ToString(), out result);

            return result;
        }

        public static byte ToByte(this object value)
        {
            byte result = 0;

            if (value != null && value.ToString().IsNumeric())
                byte.TryParse(value.ToString(), out result);

            return result;
        }
        public static bool ToBoolean(this object value)
        {
            if (value != null)
            {
                if (value.ToString().IsNumeric())
                    return value.ToInt64() > 0;
                else if (value.ToString().ToLower().Equals("true"))
                    return true;
                else if (value.ToString().ToLower().Equals("yes"))
                    return true;
            }
            return false;
        }
    }
}
