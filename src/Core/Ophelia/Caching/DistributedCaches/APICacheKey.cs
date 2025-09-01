using System;
using System.Runtime.Serialization;

namespace Ophelia.Caching.DistributedCaches
{
    [DataContract]
    public class APICacheKey
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public DateTime Expiration { get; set; }

        [DataMember]
        public object Value { get; set; }

        [DataMember]
        public bool ExtendExpirationOnGet { get; set; }
    }
}
