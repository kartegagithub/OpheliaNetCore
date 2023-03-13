using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

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
