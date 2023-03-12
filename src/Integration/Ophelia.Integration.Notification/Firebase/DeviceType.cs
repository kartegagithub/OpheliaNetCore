using System;

namespace Ophelia.Integration.Notification.Firebase
{
    public enum DeviceTypeStatusType : byte
    {
        None = 0,

        Android = 1,

        IOS = 2
    }

    public class DeviceTypeStatusValue
    {
        public static byte None
        {
            get { return Convert.ToByte(DeviceTypeStatusType.None); }
        }
        public static byte Android
        {
            get { return Convert.ToByte(DeviceTypeStatusType.Android); }
        }
        public static byte IOS
        {
            get { return Convert.ToByte(DeviceTypeStatusType.IOS); }
        }
    }
}
