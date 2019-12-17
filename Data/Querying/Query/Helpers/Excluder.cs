using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace Ophelia.Data.Querying.Query.Helpers
{
    [DataContract]
    public class Excluder : IDisposable
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public bool ExcludeFromAllQueries { get; set; }

        [DataMember]
        public Excluder SubExcluder { get; set; }

        public static Excluder Create(Expression expression, bool excludeFromAllQueries)
        {
            return ExpressionParser.Create(expression).ToExcluder(excludeFromAllQueries);
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        public Excluder Serialize()
        {
            var entity = new Excluder();
            entity.Name = this.Name;
            if (this.SubExcluder != null)
                entity.SubExcluder = this.SubExcluder.Serialize();
            return entity;
        }
    }
}
