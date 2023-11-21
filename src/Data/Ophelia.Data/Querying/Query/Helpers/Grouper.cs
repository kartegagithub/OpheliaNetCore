using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace Ophelia.Data.Querying.Query.Helpers
{
    [DataContract]
    public class Grouper : IDisposable
    {
        [DataMember]
        public string Name { get; set; } = "";

        [DataMember]
        public string TypeName { get; set; } = "";

        [XmlIgnore]
        public PropertyInfo? PropertyInfo { get; set; }

        [DataMember]
        public Grouper? SubGrouper { get; set; }
        public List<MemberInfo>? Members { get; set; }
        public Dictionary<MemberInfo, Expression>? BindingMembers { get; set; }
        public static Grouper Create(Expression expression)
        {
            return ExpressionParser.Create(expression).ToGrouper();
        }

        public string Build(BaseQuery query, bool selecting = false)
        {
            if (!string.IsNullOrEmpty(this.Name))
            {
                if (this.PropertyInfo != null)
                    return query.Data.MainTable.Alias + "." + query.Context.Connection.FormatDataElement(query.Context.Connection.GetMappedFieldName(Extensions.GetColumnName(this.PropertyInfo)));
                else
                    return query.Data.MainTable.Alias + "." + query.Context.Connection.FormatDataElement(query.Context.Connection.GetMappedFieldName(this.Name));
            }
            else if (this.BindingMembers != null && this.BindingMembers.Any())
            {
                var sb = new StringBuilder();
                var counter = 0;
                foreach (var item in this.BindingMembers)
                {
                    if (counter > 0)
                        sb.Append(",");

                    var member = (item.Value as MemberExpression).Member;
                    Includer includer = null;
                    if (member.DeclaringType != query.Data.MainTable.EntityType && query.Data.Includers != null)
                        includer = query.Data.Includers.FirstOrDefault(op => op.PropertyInfo.PropertyType == member.DeclaringType);

                    var table = query.Data.MainTable;
                    if (includer != null)
                        table = includer.Table;

                    sb.Append(table.Alias + "." + query.Context.Connection.FormatDataElement(query.Context.Connection.GetMappedFieldName(Extensions.GetColumnName(member))));
                    if (selecting)
                        sb.Append(" AS " + query.Context.Connection.FormatDataElement(query.Context.Connection.GetMappedFieldName(Extensions.GetColumnName(item.Key))));
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
            if (this.Members != null && this.Members.Any())
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
