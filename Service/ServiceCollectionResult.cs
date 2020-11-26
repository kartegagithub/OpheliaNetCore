using System.Runtime.Serialization;

namespace Ophelia.Service
{
    [DataContract(IsReference = true)]
    public class ServiceCollectionResult : ServiceResult
    {
        [DataMember]
        public object RawData { get; set; }

        [DataMember]
        public long TotalDataCount { get; set; }

        public void SetRawData(object data)
        {
            this.RawData = data;
        }
    }
}
