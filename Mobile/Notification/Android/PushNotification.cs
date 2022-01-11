using Ophelia.Net.Http;
using Ophelia.Service;
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Ophelia.Mobile.Notification.Android
{
    public class PushNotification : Mobile.Notification.PushNotification
    {
        public string URL { get; set; }

        public string GetPostData(string Reference = "", long UserID = 0, string Action = "", string CommunicationKey = "", string Title = "", int BadgeCount = 0, string Message = "")
        {
            var data = "{Reference:\"" + Reference + "\",UserID: " + UserID + ",Action:\"" + Action + "\"}";
            var PostData = "{ \"registration_ids\": [ \"" + CommunicationKey + "\" ], \"data\": {\"data\":\"" + data + "\", \"title\":\"" + Title + "\", \"badge\":\"" + BadgeCount + "\", \"message\": \"" + Message + "\"}}";

            return PostData;
        }

        public ServiceObjectResult<bool> SendGCMNotification(string ApiKey, string PostData, string PostDataContentType = "application/json")
        {
            var Result = new ServiceObjectResult<bool>();
            try
            {
                var factory = new RequestFactory()
                            .CreateClient()
                            .SetAuthorization("", string.Format("Authorization: key={0}", ApiKey))
                            .CreateRequest(this.URL, "POST")
                            .CreateStringContent(PostData, PostDataContentType);
                factory.GetStringResponse();
                Result.SetData(true);
            }
            catch (Exception ex)
            {
                Result.Fail(ex);
            }
            return Result;
        }
    }
}
