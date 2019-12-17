using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using Newtonsoft.Json.Linq;
using Ophelia;
using PushSharp.Apple;
using static PushSharp.Apple.ApnsConfiguration;
using Ophelia.Service;

namespace Ophelia.Mobile.Notification.iOS
{
    public class PushNotification : Mobile.Notification.PushNotification
    {
        private const string ProductionHost = "gateway.push.apple.com";
        private const string SandboxHost = "gateway.sandbox.push.apple.com";
        private const int NotificationPort = 2195;

        private const string ProductionFeedbackHost = "feedback.push.apple.com";
        private const string SandboxFeedbackHost = "feedback.sandbox.push.apple.com";
        private const int FeedbackPort = 2196;

        private JObject GetApnJson(string Title, string Body, string Reference, string Action, long UserID, string Sound = "chime.aiff", int BadgeCount = 0)
        {
            var apsData = new
            {
                aps = new
                {
                    sound = Sound,
                    badge = BadgeCount,
                    alert = new
                    {
                        title = Title,
                        body = Body,
                        data = new
                        {
                            ReferenceID = Reference,
                            UserID = UserID,
                            Action = Action
                        }
                    }
                }
            };
            return JObject.Parse(apsData.ToJson());
        }

        public ServiceObjectResult<bool> SendAPSNNotification(bool SandboxMode, string CertificatePath, string CertificatePassword, Dictionary<long, string> Users, string Title, string Message, string Reference = "", string Action = "")
        {
            var result = new ServiceObjectResult<bool>();
            try
            {
                var config = new ApnsConfiguration(SandboxMode ? ApnsServerEnvironment.Sandbox : ApnsServerEnvironment.Production, CertificatePath, CertificatePassword);
                var apnsBroker = new ApnsServiceBroker(config);
                apnsBroker.OnNotificationFailed += (notification, aggregateEx) =>
                {
                    aggregateEx.Handle(ex =>
                    {
                        result.Fail(ex);
                        if (ex is ApnsNotificationException)
                        {
                            var notificationException = (ApnsNotificationException)ex;
                            var apnsNotification = notificationException.Notification;
                            var statusCode = notificationException.ErrorStatusCode;
                            Console.WriteLine($"Apple Notification Failed: ID={apnsNotification.Identifier}, Code={statusCode}");
                        }
                        else
                        {
                            Console.WriteLine($"Apple Notification Failed for some unknown reason : {ex.InnerException}");
                        }
                        return true;
                    });
                };

                apnsBroker.OnNotificationSucceeded += (notification) =>
                {
                    Console.WriteLine("Apple Notification Sent!");
                };

                apnsBroker.Start();

                foreach (var user in Users)
                {
                    apnsBroker.QueueNotification(new ApnsNotification
                    {
                        DeviceToken = user.Value,
                        Payload = this.GetApnJson(Title, Message, Reference, Action, user.Key)
                    });
                }

                apnsBroker.Stop();
            }
            catch (Exception ex)
            {
                result.Fail(ex);
            }
            return result;
        }
    }
}
