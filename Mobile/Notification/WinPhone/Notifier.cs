using System;

namespace Ophelia.Mobile.Notification.WinPhone
{
    public class Notifier : Mobile.Notification.Notifier
    {
        public Notifier()
        {

        }

        public bool Send(string CommunicationKey, string Title, string Message)
        {
            PushNotification Notification = new PushNotification();
            Notification.SendToast(new Uri(CommunicationKey), Title, Message);
            return true;
        }
    }
}
