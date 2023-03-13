﻿using Ophelia.Integration.Notification.WebPush.Util;

namespace Ophelia.Integration.Notification.WebPush.Model
{
    // @LogicSoftware
    // Originally From: https://github.com/LogicSoftware/WebPushEncryption/blob/master/src/EncryptionResult.cs
    public class EncryptionResult
    {
        public byte[] PublicKey { get; set; }
        public byte[] Payload { get; set; }
        public byte[] Salt { get; set; }

        public string Base64EncodePublicKey()
        {
            return UrlBase64.Encode(PublicKey);
        }

        public string Base64EncodeSalt()
        {
            return UrlBase64.Encode(Salt);
        }
    }
}