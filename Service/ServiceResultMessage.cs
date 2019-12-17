using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Ophelia.Service
{
    [DataContract(IsReference = true)]
    public class ServiceResultMessage
    {
        [DataMember]
        public string Code { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public bool IsWarning { get; set; }
        [DataMember]
        public bool IsError { get; set; }
        [DataMember]
        public bool IsSuccess { get; set; }
    }
}
