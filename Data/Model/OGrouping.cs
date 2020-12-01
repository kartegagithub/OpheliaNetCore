using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Ophelia.Data.Model
{
    [DataContract(IsReference = true)]
    [Serializable]
    [JsonObject(MemberSerialization.OptOut)]
    public class OGrouping<TKey, TElement> : IGrouping<TKey, TElement>
    {
        [DataMember]
        public IEnumerable<TElement> Value { get; set; }

        [DataMember]
        public int Count { get; internal set; }
        
        [DataMember]
        public TKey Key { get; private set; }

        public OGrouping(TKey key, IEnumerable<TElement> values, long count = 0) : this(key, values, Convert.ToInt32(count))
        {

        }
        public OGrouping(TKey key, IEnumerable<TElement> values, int count = 0)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            this.Key = key;
            this.Value = values;
            this.Count = count;
            if (count == 0)
                this.Count = this.Value.Count();
        }
        public OGrouping(IGrouping<TKey, TElement> grouping)
        {
            if (grouping == null)
                throw new ArgumentNullException("grouping");
            Key = grouping.Key;
            Value = grouping.ToList();
        }

        public OGrouping()
        {
            
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return this.Value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

    }
}
