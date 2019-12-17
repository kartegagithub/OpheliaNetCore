using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
namespace Ophelia.Mobile.Notification.WinPhone
{
    //Info: http://www.yazgelistir.com/makale/windows-phone-7-push-notification-types
    public class PushNotification: Mobile.Notification.PushNotification
    {
        public PushNotification()
        {

        }

        public bool SendToast(Uri PhoneURI, string MessageTitle, string MessageSubTitle)
        {
            string msg =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
            "<wp:Notification xmlns:wp=\"WPNotification\">" +
            "<wp:Toast>" +
            "<wp:Text1>" + MessageTitle + "</wp:Text1>" +
            "<wp:Text2>" + MessageSubTitle + "</wp:Text2>" +
            "</wp:Toast>" +
            "</wp:Notification>";
            byte[] data = new UTF8Encoding().GetBytes(msg);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(PhoneURI);
            request.Method = WebRequestMethods.Http.Post;
            request.ContentLength = data.Length;
            request.Headers["X-MessageID"] = Guid.NewGuid().ToString();
            request.Headers["X-WindowsPhone-Target"] = "toast";
            request.ContentType = "text/xml";
            request.Headers["X-NotificationClass"] = "2";
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(data, 0, data.Length);
            requestStream.Close();
            //http://msdn.microsoft.com/en-us/library/windowsphone/develop/ff941100(v=vs.105).aspx
            //Değerler: (Connected|InActive|Disconnected|TempDisconnected)
            //HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            //response.Header["X-DeviceConnectionStatus"];
            return true;
        }
    }
}
