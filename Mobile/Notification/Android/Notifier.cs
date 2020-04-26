using Ophelia.Service;

namespace Ophelia.Mobile.Notification.Android
{
    public class Notifier : Mobile.Notification.Notifier
    {
        public string URL { get; set; }
        public Notifier()
        {
            this.URL = "https://android.googleapis.com/gcm/send";
        }

        public ServiceObjectResult<bool> Send(string ApiKey, string CommunicationKey, string Message, string Title = "", long UserID = 0, string Reference = "", string Action = "", int BadgeCount = 0)
        {
            var notification = new PushNotification();
            notification.URL = this.URL;
            var data = notification.GetPostData(Reference, UserID, Action, CommunicationKey, Title, BadgeCount, Message);
            return notification.SendGCMNotification(ApiKey, data);
        }
        public Notifier(string url)
        {
            this.URL = url;
        }
    }
}
