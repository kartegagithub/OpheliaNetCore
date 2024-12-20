using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Ophelia.Integration.FacebookAuth.Model;

namespace Ophelia.Integration.FacebookAuth
{
    public class FacebookLoginService
    {
        public FacebookLoginService(string appID, string appSecret)
        {
            AppID = appID;
            AppSecret = appSecret;
        }

        private string AppID = "";
        private string AppSecret = "";

        public List<FacebookAuthResponse> ValidateFacebookToken(string token, string fields = "id,name,email")
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new Exception("TokenIsRequired.");
            }

            var tokenValidationUrl = $"https://graph.facebook.com/debug_token?input_token={token}&access_token={AppID}|{AppSecret}";

            var tokenValidationResponse = tokenValidationUrl.DownloadURL();
            var tokenValidationData = JObject.Parse(tokenValidationResponse);

            if (tokenValidationData["data"]?["is_valid"]?.Value<bool>() != true)
            {
                throw new Exception("InvalidOrExpiredAccessToken");
            }

            var userInfoUrl = $"https://graph.facebook.com/me?fields={fields}&access_token={token}";
            var userInfoResponse = userInfoUrl.DownloadURL();
            var userInfo = JObject.Parse(userInfoResponse);

            if (userInfo["error"] != null)
            {
                throw new Exception("FailedToFetchUserInfo");
            }

            var response = new List<FacebookAuthResponse>();
            if (!string.IsNullOrEmpty(fields))
            {
                var fieldList = fields.Split(',');
                foreach (var field in fieldList)
                {
                    response.Add(new FacebookAuthResponse { Type = field, Value = userInfo[field]?.Value<string>() });
                }
            }
            return response;
        }
    }
}