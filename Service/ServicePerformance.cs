using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Service
{
    [DataContract(IsReference = true)]
    public class ServicePerformance
    {
        private DateTime endDate = DateTime.MinValue;

        [DataMember]
        public int QueryCount { get; set; }

        [DataMember]
        public List<string> Queries { get; set; }

        [DataMember]
        public DateTime StartDate { get; set; }

        [DataMember]
        public DateTime EndDate
        {
            get
            {
                if (this.endDate != DateTime.MinValue)
                    return this.endDate;
                else
                    return DateTime.Now;
            }
            set
            {
                this.endDate = value;
            }
        }
        [DataMember]
        public double Duration
        {
            get
            {
                return this.EndDate.Subtract(this.StartDate).TotalMilliseconds;
            }
            set
            {
            }
        }
        public ServicePerformance()
        {
            this.StartDate = DateTime.Now;
        }
    }
}
