using System.Collections.Generic;

namespace Ophelia.Mobile.Notification.Expo
{
    public class ExpoNotificationResult
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
