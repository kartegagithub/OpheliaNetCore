using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ophelia.Service
{
    [DataContract(IsReference = true)]
    public class ServiceCollectionResult<TEntity> : ServiceCollectionResult
    {
        [DataMember]
        public List<TEntity> Data { get; set; }

        public void SetData(int totalCount, List<TEntity> list)
        {
            this.SetData(Convert.ToInt64(totalCount), list);
        }

        public void SetData(long totalCount, IList list)
        {
            if (list is List<TEntity>)
                this.SetData(totalCount, list as List<TEntity>);
            else
            {
                this.TotalDataCount = totalCount;
                this.RawData = list;
                if (!this.HasFailed)
                    this.HasFailed = false;
            }
        }

        public void SetData(long totalCount, List<TEntity> list)
        {
            this.TotalDataCount = totalCount;
            this.Data = list;
            if (!this.HasFailed)
                this.HasFailed = false;
        }

        public void SetData(List<TEntity> list)
        {
            this.Data = list;
            if (list != null)
                this.TotalDataCount = list.Count;
            if (!this.HasFailed)
                this.HasFailed = false;
        }

        private bool _HasData;
        [DataMember]
        public bool HasData
        {
            get
            {
                if (this.Data != null && this.Data.Count > 0)
                    this._HasData = true;
                return this._HasData;
            }
            set
            {
                this._HasData = value;
            }
        }
    }
}
