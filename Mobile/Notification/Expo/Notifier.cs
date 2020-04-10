using Ophelia;
using Ophelia.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Ophelia.Mobile.Notification.Expo
{
    public class Notifier : Mobile.Notification.Notifier
    {
        public string URL { get; set; }
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
                Result.Fail(ex);
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
                Result.Fail(ex);
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
                Result.Fail(ex);
            }
            return Result;
        }

        public Notifier()
        {
            this.URL = "https://exp.host/--/api/v2/push/send";
        }
    }
}
