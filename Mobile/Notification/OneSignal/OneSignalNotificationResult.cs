using System.Collections.Generic;

namespace Ophelia.Mobile.Notification.OneSignal
{
    public class OneSignalNotificationResult
    {
        public string id { get; set; }
        public int recipients { get; set; }
        public string external_id { get; set; }
        public List<string> errors { get; set; }

        public OneSignalNotificationResult()
        {
            this.errors = new List<string>();
        }
    }
}
