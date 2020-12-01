using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Ophelia.Data.Querying.Query.Helpers
{
    [DataContract]
    public class Grouper : IDisposable
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string TypeName { get; set; }

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
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {

        }
        public List<Grouper> Serialize()
        {
            var groupers = new List<Grouper>();
            if (!string.IsNullOrEmpty(this.Name))
            {
                groupers.Add(new Grouper() { Name = this.Name, TypeName = this.TypeName });
            }
            if(this.Members != null && this.Members.Any())
            {
                foreach (var item in this.Members)
                {
                    groupers.Add(new Grouper() { Name = item.Name, TypeName = item.GetMemberInfoType().FullName });
                }
            }
            if (this.SubGrouper != null)
                groupers.AddRange(this.SubGrouper.Serialize());
            return groupers;
        }
    }
}
