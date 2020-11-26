using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ophelia.Data.Querying.Query
{
    [DataContract]
    public class QueryData : IDisposable
    {
        internal Type EntityType { get; set; }

        [DataMember]
        public int GroupPageSize { get; set; }

        [DataMember]
        public Dictionary<int, int> GroupPagination { get; set; }

        [DataMember]
        public int PageSize { get; set; }

        [DataMember]
        public int SkippedCount { get; set; }

        [DataMember]
        public Helpers.Filter Filter { get; set; }

        [DataMember]
        public List<Helpers.Includer> Includers { get; set; }

        [DataMember]
        public List<Helpers.Excluder> Excluders { get; set; }

        [DataMember]
        public List<Helpers.Sorter> Sorters { get; set; }

        [DataMember]
        public List<Helpers.DBFunction> Functions { get; set; }

        [DataMember]
        public List<Helpers.Grouper> Groupers { get; set; }

        [DataMember]
        public List<Helpers.Selector> Selectors { get; set; }

        [DataMember]
        public List<object> Parameters { get; set; }

        [DataMember]
        public Helpers.Table MainTable { get; set; }
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.Sorters = null;
            this.Groupers = null;
            this.Includers = null;
            this.Selectors = null;
            this.Parameters = null;
            this.Functions = null;
            this.Excluders = null;
            this.GroupPagination = null;
        }

        public QueryData()
        {
            this.Sorters = new List<Helpers.Sorter>();
            this.Groupers = new List<Helpers.Grouper>();
            this.Includers = new List<Helpers.Includer>();
            this.Selectors = new List<Helpers.Selector>();
            this.Parameters = new List<object>();
            this.Functions = new List<Helpers.DBFunction>();
            this.Excluders = new List<Helpers.Excluder>();
            this.GroupPagination = new Dictionary<int, int>();
        }
        public QueryData Serialize()
        {
            var qd = new QueryData();
            foreach (var item in this.Sorters)
            {
                qd.Sorters.Add(item.Serialize());
            }
            foreach (var item in this.Groupers)
            {
                qd.Groupers.AddRange(item.Serialize());
            }
            foreach (var item in this.Includers)
            {
                qd.Includers.Add(item.Serialize());
            }
            foreach (var item in this.Selectors)
            {
                qd.Selectors.Add(item.Serialize());
            }
            foreach (var item in this.Functions)
            {
                qd.Functions.Add(item.Serialize());
            }
            foreach (var item in this.Parameters)
            {
                qd.Parameters.Add(item);
            }
            if (this.Filter != null)
                qd.Filter = this.Filter.Serialize();

            qd.PageSize = this.PageSize;
            qd.SkippedCount = this.SkippedCount;
            qd.GroupPageSize = this.GroupPageSize;
            qd.GroupPagination = this.GroupPagination;
            return qd;
        }
    }
}
