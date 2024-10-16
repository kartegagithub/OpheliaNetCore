using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ophelia.Service
{
    [DataContract(IsReference = true)]
    public class ServicePerformance
    {
        private DateTime endDate = DateTime.MinValue;
        private double _Duration;

        [DataMember]
        public int QueryCount { get; set; }

        [DataMember]
        public List<Data.Model.SQLLog> SQLLogs { get; set; }

        [DataMember]
        public List<Data.Model.EntityLoadLog> EntityLoadLogs { get; set; }

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
                    return Utility.Now;
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
            this.StartDate = Utility.Now;
            this.SQLLogs = new List<Data.Model.SQLLog>();
            this.EntityLoadLogs = new List<Data.Model.EntityLoadLog>();
        }
    }
}
