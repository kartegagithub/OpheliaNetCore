using System;

namespace Ophelia.Integration.Notification.OneSignal
{
    public class Notifier : Notification.Notifier
    {
        public string URL { get; set; }
        public OneSignalNotificationResult SendToMultipleDevice(string[] registrationIds, string title, string body, string appId, string androidChannelID, string type = "", dynamic extraData = null, string EntityType = "", long EntityID = 0, long BadgeCount = 0, string webURL = "")
        {
            var Result = new OneSignalNotificationResult();
            try
            {
                var data = new
                {
                    app_id = appId,
                    android_channel_id = androidChannelID,
                    include_player_ids = registrationIds,
                    headings = new
                    {
                        en = title
                    },
                    contents = new
                    {
                        en = body
                    },
                    ios_badgeType = "SetTo",
                    ios_badgeCount = BadgeCount,
                    web_url = webURL,
                    data = new
                    {
                        body = body,
                        title = title,
                        type = type,
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
        public OneSignalNotificationResult SendToSingleDevice(string token, string title, string body, string appId, string androidChannelID, string type = "", dynamic extraData = null, string EntityType = "", long EntityID = 0, long BadgeCount = 0, string webURL = "")
        {
            var Result = new OneSignalNotificationResult();
            try
            {
                var data = new
                {
                    app_id = appId,
                    android_channel_id = androidChannelID,
                    include_player_ids = new string[] { token },
                    headings = new
                    {
                        en = title
                    },
                    contents = new
                    {
                        en = body
                    },
                    ios_badgeType = "SetTo",
                    ios_badgeCount = BadgeCount,
                    web_url = webURL,
                    data = new
                    {
                        body = body,
                        title = title,
                        type = type,
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
        public OneSignalNotificationResult Send(string data, string postDataContentType = "application/json")
        {
            var Result = new OneSignalNotificationResult();
            try
            {
                Result = this.URL.PostURL<OneSignalNotificationResult>(data, "application/json");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return Result;
        }

        public Notifier()
        {
            this.URL = "https://onesignal.com/api/v1/notifications";
        }
    }
}
