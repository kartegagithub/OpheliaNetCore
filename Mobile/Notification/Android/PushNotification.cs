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
        /// <summary>
        /// This method is used to integrate Android Push Notification
        /// </summary>
        /// <param name="RegistrationID">Registration ID or Token</param>
        /// <param name="SenderID">Google EmailID</param>
        /// <param name="Password">Password of EmailID</param>
        /// <param name="Message">Push Message</param>
        /// <returns>Status=Provided parameter missing the currect value,Authentication Fail, Unauthorized - need new token, Response from web service isn't OK, Success</returns>
        //public string Send(string RegistrationID, string SenderID, string Password, string Message)
        //{
        //    string Status = "";
        //    //--Validating the required parameter--//
        //    if (CheckAndroidValidation(RegistrationID, SenderID, Password, Message) == false)
        //    {
        //        Status = "Provided parameter missing the currect value.";
        //    }
        //    else
        //    {
        //        //-- Check Authentication --//
        //        Android objAndroid = new Android();
        //        string AuthString = objAndroid.CheckAuthentication(SenderID, Password);

        //        if (AuthString == "Fail")
        //        {
        //            Status = "Authentication Fail";
        //        }
        //        else
        //        {
        //            Status = objAndroid.SendMessage(RegistrationID, Message, AuthString);
        //        }

        //    }

        //    //-- Return the Status of Push Notification --//
        //    return Status;
        //}

        /// <summary>
        /// Check Parameter Validation for Android 
        /// </summary>
        /// <returns></returns>
        //private bool CheckAndroidValidation(string RegistrationID, string SenderID, string Password, string Message)
        //{
        //    bool RetValue = true;

        //    if (RegistrationID.Trim() == "")
        //    {
        //        RetValue = false;
        //    }

        //    if (SenderID.Trim() == "")
        //    {
        //        RetValue = false;
        //    }

        //    if (Password.Trim() == "")
        //    {
        //        RetValue = false;
        //    }

        //    if (Message.Trim() == "")
        //    {
        //        RetValue = false;
        //    }

        //    return RetValue;
        //}

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
                ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateServerCertificate);
                byte[] byteArray = Encoding.UTF8.GetBytes(PostData);
                HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(this.URL);
                Request.Method = "POST";
                Request.KeepAlive = false;
                Request.ContentType = PostDataContentType;
                Request.Headers.Add(string.Format("Authorization: key={0}", ApiKey));
                Request.ContentLength = byteArray.Length;
                Stream dataStream = Request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                try
                {
                    WebResponse Response = Request.GetResponse();
                    StreamReader Reader = new StreamReader(Response.GetResponseStream());
                    string responseLine = Reader.ReadToEnd();
                    Reader.Close();
                }
                catch (Exception ex)
                {
                    Result.Fail(ex);
                }
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
    }
}
