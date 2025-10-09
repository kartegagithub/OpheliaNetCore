using System.Collections.Generic;

namespace Ophelia.Integration.Notification.OneSignal
{
    public class OneSignalNotificationResult
    {
        public string id { get; set; }
        public int recipients { get; set; }
        public string external_id { get; set; }
        public OneSignalNotificationErrorResult errors { get; set; }

        public OneSignalNotificationResult()
        {
        }
    }

    public class OneSignalNotificationErrorResult
    {
        public List<string> invalid_player_ids { get; set; }

        public OneSignalNotificationErrorResult()
        {
            this.invalid_player_ids = new List<string>();
        }
    }
}
