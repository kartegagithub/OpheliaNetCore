using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ophelia.Service
{
    [DataContract(IsReference = true)]
    public class ServicePerformance
    {
        private DateTime endDate = DateTime.MinValue;
        private double _Duration = 0;

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
                _Duration = this.EndDate.Subtract(this.StartDate).TotalMilliseconds;
                return _Duration;
            }
            set
            {
                this._Duration = value;
            }
        }
        public ServicePerformance()
        {
            this.StartDate = DateTime.Now;
        }
    }
}
