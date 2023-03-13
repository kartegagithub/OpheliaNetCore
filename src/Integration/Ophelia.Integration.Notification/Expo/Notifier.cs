using System;

namespace Ophelia.Integration.Notification.Expo
{
    public class Notifier : Notification.Notifier
    {
        public string URL { get; set; } = "";
        public ExpoNotificationResult SendToMultipleDevice(string[] registrationIds, string title, string body, string type = "notification", dynamic extraData = null, string priority = "high", bool content_available = true, string EntityType = "", long EntityID = 0, long BadgeCount = 0)
        {
            var Result = new ExpoNotificationResult();
            try
            {
                var data = new
                {
                    to = registrationIds,
                    body = body,
                    title = title,
                    sound = "default",
                    priority = priority,
                    badge = BadgeCount,
                    data = new
                    {
                        body = body,
                        title = title,
                        type = type,
                        content_available = true,
                        badge = BadgeCount,
                        extraData = extraData,
                        entityID = EntityID,
                        entityType = EntityType
                    }
                };
                return this.Send(data.ToJson());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return Result;
        }
        public ExpoNotificationResult SendToSingleDevice(string token, string title, string body, string type = "notification", dynamic extraData = null, string priority = "high", string EntityType = "", long EntityID = 0, long BadgeCount = 0)
        {
            var Result = new ExpoNotificationResult();
            try
            {
                var data = new
                {
                    to = token,
                    body = body,
                    title = title,
                    sound = "default",
                    priority = priority,
                    badge = BadgeCount,
                    data = new
                    {
                        body = body,
                        title = title,
                        type = type,
                        content_available = true,
                        badge = BadgeCount,
                        extraData = extraData,
                        entityID = EntityID,
                        entityType = EntityType
                    }
                };
                return this.Send(data.ToJson());

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return Result;
        }
        public ExpoNotificationResult Send(string data, string postDataContentType = "application/json")
        {
            var Result = new ExpoNotificationResult();
            try
            {
                Result = this.URL.PostURL<ExpoNotificationResult>(data, "application/json");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return Result;
        }

        public Notifier()
        {
            this.URL = "https://exp.host/--/api/v2/push/send";
        }
    }
}
