using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Data.Model
{
    public class OGrouping<TKey, TElement> : IGrouping<TKey, TElement>
    {
        readonly IEnumerable<TElement> elements;
        public int Count { get; internal set; }
        public OGrouping(TKey key, IEnumerable<TElement> values, long count = 0) : this(key, values, Convert.ToInt32(count))
        {

        }
        public OGrouping(TKey key, IEnumerable<TElement> values, int count = 0)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            this.Key = key;
            this.elements = values;
            this.Count = count;
            if (count == 0)
                this.Count = this.elements.Count();
        }
        public OGrouping(IGrouping<TKey, TElement> grouping)
        {
            if (grouping == null)
                throw new ArgumentNullException("grouping");
            Key = grouping.Key;
            elements = grouping.ToList();
        }

        public TKey Key { get; private set; }

        public IEnumerator<TElement> GetEnumerator()
        {
            return this.elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

    }
}
