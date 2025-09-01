namespace Ophelia
{
    public static class ObjectExtensions
    {
        public static short ToShort(this object value)
        {
            return value.ToInt16();
        }
        public static short ToInt16(this object value)
        {
            return value != null ? value.GetType().ToInt16(value) : (short)0;
        }

        public static int ToInt32(this object value)
        {
            return value != null ? value.GetType().ToInt32(value) : 0;
        }

        public static long ToInt64(this object value)
        {
            return value != null ? value.GetType().ToInt64(value) : 0;
        }

        public static byte ToByte(this object value)
        {
            return value != null ? value.GetType().ToByte(value) : (byte)0;
        }
        public static bool ToBoolean(this object value)
        {
            return value != null ? value.GetType().ToBoolean(value) : false;
        }
    }
}
