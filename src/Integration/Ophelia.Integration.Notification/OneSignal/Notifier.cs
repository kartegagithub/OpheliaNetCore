using System;
using System.Net;

namespace Ophelia.Integration.Notification.OneSignal
{
    public class Notifier : Notification.Notifier
    {
        public string URL { get; set; }
        public string APIKey { get; set; }
        public OneSignalNotificationResult SendToMultipleDevice(string[] registrationIds, string title, string body, string appId, string type = "", dynamic extraData = null, string EntityType = "", long EntityID = 0, long BadgeCount = 0, string webURL = "", string imagePath = "")
        {
            var Result = new OneSignalNotificationResult();
            try
            {
                var data = new
                {
                    app_id = appId,
                    include_player_ids = registrationIds,
                    headings = new
                    {
                        en = title
                    },
                    contents = new
                    {
                        en = body
                    },
                    ios_attachments = new
                    {
                        id = imagePath
                    },
                    big_picture = imagePath,
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
        public OneSignalNotificationResult SendToSingleDevice(string token, string title, string body, string appId, string type = "", dynamic extraData = null, string EntityType = "", long EntityID = 0, long BadgeCount = 0, string webURL = "", string imagePath = "")
        {
            var Result = new OneSignalNotificationResult();
            try
            {
                var data = new
                {
                    app_id = appId,
                    include_player_ids = new string[] { token },
                    headings = new
                    {
                        en = title
                    },
                    contents = new
                    {
                        en = body
                    },
                    ios_attachments = new
                    {
                        id = imagePath
                    },
                    big_picture = imagePath,
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
                var headers = new WebHeaderCollection
                {
                    { "Accept", "application/json" },
                    { "Content-Type", "application/json" },
                    { "Authorization", $"Basic {this.APIKey}" }
                };
                Result = this.URL.PostURL<OneSignalNotificationResult>(data, "application/json", headers);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return Result;
        }

        public Notifier(string APIKey)
        {
            this.URL = "https://onesignal.com/api/v1/notifications";
            this.APIKey = APIKey;
        }
    }
}
