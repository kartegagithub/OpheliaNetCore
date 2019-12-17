using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Mobile.Notification.Firebase
{
    public class FirebaseNotificationResult : Ophelia.Service.ServiceObjectResult<bool>
    {
        public FirebaseNotificationResultData ResultData { get; set; }
    }
    public class FirebaseNotificationResultData
    {
        public string multicast_id { get; set; }
        public int success { get; set; }
        public int failure { get; set; }
        public int canonical_ids { get; set; }
        public List<FirebaseNotificationResultMessage> results { get; set; }
    }
    public class FirebaseNotificationResultMessage
    {
        public string message_id { get; set; }
        public string error { get; set; }
    }
}
