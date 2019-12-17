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
    public class Selector : IDisposable
    {
        [DataMember]
        public string Name { get; set; }

        public PropertyInfo PropertyInfo { get; set; }
        public List<MemberInfo> Members { get; set; }

        [DataMember]
        public Selector SubSelector { get; set; }
        public static Selector Create(Expression expression)
        {
            return ExpressionParser.Create(expression).ToSelector();
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public string Build(BaseQuery query)
        {
            if (!string.IsNullOrEmpty(this.Name))
            {
                if (this.PropertyInfo == null || this.PropertyInfo.PropertyType.IsPrimitiveType())
                    return query.Data.MainTable.Alias + "." + query.Context.Connection.FormatDataElement(query.Context.Connection.GetMappedFieldName(this.Name));
            }
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
            return "";
        }
        public Selector()
        {

        }
        public Selector Serialize()
        {
            var entity = new Selector();
            entity.Name = this.Name;
            if (this.SubSelector != null)
                entity.SubSelector = this.SubSelector.Serialize();
            return entity;
        }
    }
}
