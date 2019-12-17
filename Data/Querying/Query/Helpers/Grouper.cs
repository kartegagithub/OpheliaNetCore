using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Reflection;

namespace Ophelia.Data.Querying.Query.Helpers
{
    [DataContract]
    public class Grouper : IDisposable
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public Grouper SubGrouper { get; set; }
        public List<MemberInfo> Members { get; set; }
        public static Grouper Create(Expression expression)
        {
            return ExpressionParser.Create(expression).ToGrouper();
        }

        public string Build(BaseQuery query)
        {
            if (!string.IsNullOrEmpty(this.Name))
                return query.Data.MainTable.Alias + "." + query.Context.Connection.FormatDataElement(query.Context.Connection.GetMappedFieldName(this.Name));
            else if (this.Members != null && this.Members.Count > 0)
            {
                var sb = new StringBuilder();
                var counter = 0;
                foreach (var item in this.Members)
                {
                    if (counter > 0)
                        sb.Append(",");
                    sb.Append(query.Data.MainTable.Alias + "." + query.Context.Connection.FormatDataElement(query.Context.Connection.GetMappedFieldName(item.Name)));
                    counter++;
                }
                return sb.ToString();
            }
            else if (this.SubGrouper != null)
                return this.SubGrouper.Build(query);
            return "";
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        public Grouper Serialize()
        {
            var entity = new Grouper();
            entity.Name = this.Name;
            if (this.SubGrouper != null)
                entity.SubGrouper = this.SubGrouper.Serialize();
            return entity;
        }
    }
}
