using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Mobile.Notification.Expo
{
    public class ExpoNotificationResult : Ophelia.Service.ServiceObjectResult<bool>
    {
        public ExpoNotificationResultData data { get; set; }
    }
    public class ExpoNotificationResultData
    {
        public string status { get; set; }
        public string id { get; set; }
        public string message { get; set; }
        public List<ExpoNotificationResultMessage> details { get; set; }
    }
    public class ExpoNotificationResultMessage
    {
        public string error { get; set; }
    }
}
