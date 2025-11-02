using System;

namespace Ophelia
{
    public static class ObjectExtensions
    {
        public static short ToShort(this object value)
        {
            short result = 0;
            if (value != null && short.TryParse(value.ToString(), out result))
                return result;
            return result;
        }
        public static Int16 ToInt16(this object value)
        {
            Int16 result = 0;
            if (value != null && Int16.TryParse(value.ToString(), out result))
                return result;
            return result;
        }

        public static Int32 ToInt32(this object value)
        {
            Int32 result = 0;
            if (value != null && Int32.TryParse(value.ToString(), out result))
                return result;
            return result;
        }

        public static long ToInt64(this object value)
        {
            Int64 result = 0;
            if (Int64.TryParse(value.ToString(), out result))
                return result;
            return result;
        }

        public static byte ToByte(this object value)
        {
            byte result = 0;
            if (value != null && byte.TryParse(value.ToString(), out result))
                return result;
            return result;
        }
        public static bool ToBoolean(this object value)
        {
            if (value != null)
            {
                var typeCode = Type.GetTypeCode(value.GetType());
                switch (typeCode)
                {
                    case TypeCode.Boolean:
                        return (bool)value;
                    case TypeCode.Char:
                        var cValue = (char)value;
                        return cValue.Equals("Y") || cValue.Equals("y");
                    case TypeCode.String:
                        var sValue = value.ToString().ToLowerInvariant();
                        if (string.IsNullOrEmpty(sValue))
                            return false;

                        return value.Equals("true") || value.Equals("yes");
                    default:
                        return value.ToInt64() > 0;
                }
            }
            return false;
        }
    }
}
