using Ophelia.Net.Http;
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Ophelia.Mobile.Notification.Firebase
{
    public class Notifier : Mobile.Notification.Notifier
    {
        public string URL { get; set; }
        public FirebaseNotificationResult SendToMultipleDevice(string apiKey, string[] registrationIds, string title, string body, string type = "notification", dynamic extraData = null, string priority = "high", DeviceTypeStatusType deviceType = DeviceTypeStatusType.None, bool content_available = true, string EntityType = "", long EntityID = 0, long BadgeCount = 0)
        {
            var Result = new FirebaseNotificationResult();
            try
            {
                var data = new object();
                switch (deviceType)
                {
                    case DeviceTypeStatusType.Android:
                        data = new
                        {
                            registration_ids = registrationIds,
                            priority = priority,
                            data = new
                            {
                                body = body,
                                title = title,
                                type = type,
                                badge = BadgeCount,
                                content_available = content_available,
                                extraData = extraData,
                                entityID = EntityID,
                                entityType = EntityType
                            }
                        };
                        break;
                    default:
                        data = new
                        {
                            registration_ids = registrationIds,
                            priority = priority,
                            notification = new
                            {
                                body = body,
                                title = title,
                                type = type,
                                badge = BadgeCount,
                                content_available = content_available,
                                extraData = extraData,
                                entityID = EntityID,
                                entityType = EntityType
                            }
                        };
                        break;
                }
                return this.Send(apiKey, data.ToJson());
            }
            catch (Exception ex)
            {
                Result.Fail(ex);
            }
            return Result;
        }
        public FirebaseNotificationResult SendToMultipleDevice(string apiKey, string registrationIds, string title, string body, string type = "notification", dynamic extraData = null, string priority = "high", DeviceTypeStatusType deviceType = DeviceTypeStatusType.None, bool content_available = true, string EntityType = "", long EntityID = 0, long BadgeCount = 0)
        {
            return SendToMultipleDevice(apiKey, registrationIds.Split(','), title, body, type, extraData, priority, deviceType, content_available, EntityType, EntityID, BadgeCount);
        }
        public FirebaseNotificationResult SendToSingleDevice(string apiKey, string token, string title, string body, string type = "notification", dynamic extraData = null, string priority = "high", DeviceTypeStatusType deviceType = DeviceTypeStatusType.None, string EntityType = "", long EntityID = 0, long BadgeCount = 0)
        {
            var Result = new FirebaseNotificationResult();
            try
            {
                var data = new object();
                switch (deviceType)
                {
                    case DeviceTypeStatusType.Android:
                        data = new
                        {
                            to = token,
                            priority = priority,
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
                        break;
                    default:
                        data = new
                        {
                            to = token,
                            priority = priority,
                            notification = new
                            {
                                body = body,
                                title = title,
                                type = type,
                                badge = BadgeCount,
                                content_available = true,
                                extraData = extraData,
                                entityID = EntityID,
                                entityType = EntityType
                            }
                        };
                        break;
                }
                return this.Send(apiKey, data.ToJson());

            }
            catch (Exception ex)
            {
                Result.Fail(ex);
            }
            return Result;
        }
        public FirebaseNotificationResult SendToCondition(string apiKey, string condition, string title, string body, string type = "notification", dynamic extraData = null, string priority = "high", string EntityType = "", long EntityID = 0, long BadgeCount = 0)
        {
            var Result = new FirebaseNotificationResult();
            try
            {
                var data = new
                {
                    condition = condition,
                    priority = priority,
                    notification = new
                    {
                        body = body,
                        title = title,
                        type = type,
                        badge = BadgeCount,
                        content_available = true,
                        extraData = extraData,
                        entityID = EntityID,
                        entityType = EntityType
                    }
                };
                return this.Send(apiKey, data.ToJson());
            }
            catch (Exception ex)
            {
                Result.Fail(ex);
            }
            return Result;
        }
        public FirebaseNotificationResult SendToTopic(string apiKey, string topic, string title, string body, string type = "notification", dynamic extraData = null, string priority = "high", string EntityType = "", long EntityID = 0, long BadgeCount = 0)
        {
            var Result = new FirebaseNotificationResult();
            try
            {
                var data = new
                {
                    to = topic,
                    priority = priority,
                    notification = new
                    {
                        body = body,
                        title = title,
                        type = type,
                        badge = BadgeCount,
                        content_available = true,
                        extraData = extraData,
                        entityID = EntityID,
                        entityType = EntityType
                    }
                };
                return this.Send(apiKey, data.ToJson());
            }
            catch (Exception ex)
            {
                Result.Fail(ex);
            }
            return Result;
        }
        public FirebaseNotificationResult Send(string apiKey, string data, string postDataContentType = "application/json")
        {
            var Result = new FirebaseNotificationResult();
            try
            {
                var factory = new RequestFactory()
                            .CreateClient()
                            .SetAuthorization("", string.Format("Authorization: key={0}", apiKey))
                            .CreateRequest(this.URL, "POST")
                            .CreateStringContent(data, postDataContentType);
                Result.ResultData = factory.GetJsonResponse<FirebaseNotificationResultData>();
                Result.SetData(Result.ResultData.success > 0);
            }
            catch (Exception ex)
            {
                Result.Fail(ex);
            }
            return Result;
        }
        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
        public Notifier()
        {
            this.URL = "https://fcm.googleapis.com/fcm/send";
        }
    }
}
