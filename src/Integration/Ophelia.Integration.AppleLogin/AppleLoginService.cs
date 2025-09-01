using Microsoft.IdentityModel.Tokens;
using Ophelia;
using Ophelia.Integration.AppleAuth.Model;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;

namespace Ophelia.Integration.AppleAuth
{
    public class AppleAuthService
    {
        public AppleAuthService(string clientID)
        {
            ClientId = clientID;
        }

        private const string ApplePublicKeyUrl = "https://appleid.apple.com/auth/keys";
        private const string AppleIssuer = "https://appleid.apple.com";
        private string ClientId = "";

        public List<AppleAuthResponse> ValidateAppleToken(string identityToken)
        {
            var applePublicKeys = GetApplePublicKeys();

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(identityToken);

            var kid = jwtToken.Header.Kid;
            var alg = jwtToken.Header.Alg;

            var matchingKey = applePublicKeys.Keys.FirstOrDefault(k => k.Kid == kid);
            if (matchingKey == null)
                throw new Exception("Matching key not found.");

            var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(new RSAParameters
            {
                Modulus = Base64UrlDecode(matchingKey.N),
                Exponent = Base64UrlDecode(matchingKey.E)
            });

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = AppleIssuer,
                ValidateAudience = true,
                ValidAudience = ClientId,
                ValidateLifetime = true,
                IssuerSigningKey = new RsaSecurityKey(rsa)
            };

            var claimsPrincipal = tokenHandler.ValidateToken(identityToken, validationParameters, out SecurityToken validatedToken);

            var response = new List<AppleAuthResponse>();
            foreach (var claim in jwtToken.Claims)
            {
                response.Add(new AppleAuthResponse { Type = claim.Type, Value = claim.Value });
            }
            return response;
        }

        private ApplePublicKeys GetApplePublicKeys()
        {
            var response = ApplePublicKeyUrl.DownloadURL();
            return response.FromJson<ApplePublicKeys>();
        }

        private byte[] Base64UrlDecode(string input)
        {
            var output = input.Replace('-', '+').Replace('_', '/');
            switch (output.Length % 4)
            {
                case 2: output += "=="; break;
                case 3: output += "="; break;
            }
            return Convert.FromBase64String(output);
        }
    }
}