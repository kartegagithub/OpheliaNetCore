using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Mobile.Notification.Firebase
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
