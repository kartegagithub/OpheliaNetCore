using System.Collections.Generic;

namespace Ophelia.Integration.AppleAuth.Model
{
    public class AppleAuthResponse
    {
        public string Type { get; set; }
        public object Value { get; set; }
    }

    public class ApplePublicKeys
    {
        public List<ApplePublicKey> Keys { get; set; }
    }

    public class ApplePublicKey
    {
        public string Kty { get; set; }
        public string Kid { get; set; }
        public string Use { get; set; }
        public string Alg { get; set; }
        public string N { get; set; }
        public string E { get; set; }
    }
}