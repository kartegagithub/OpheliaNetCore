using Google.Apis.Auth;
using System;
using System.Collections.Generic;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace Ophelia.Integration.GoogleAuth
{
    public class GoogleLoginService
    {
        public GoogleLoginService(string clientID)
        {
            ClientId = clientID;
        }

        private string ClientId = "";

        public Payload ValidateGoogleToken(string token)
        {
            ValidationSettings? settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new List<string>() { ClientId }
            };
            Payload payload = GoogleJsonWebSignature.ValidateAsync(token, settings).Result;

            if (payload == null)
            {
                throw new UnauthorizedAccessException();
            }

            var expirationDate = DateTimeOffset.FromUnixTimeSeconds(payload.ExpirationTimeSeconds.Value).LocalDateTime;

            if (DateTime.Now > expirationDate)
            {
                throw new Exception("The token has expired.");
            }

            return payload;
        }
    }
}