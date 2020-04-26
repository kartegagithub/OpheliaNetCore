using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Ophelia.Data.Querying.Query.Helpers
{
    [DataContract]
    public class Table : IDisposable
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Alias { get; set; }

        [DataMember]
        public string FullName { get; set; }

        [DataMember]
        public string EntityTypeName { get; set; }

        public Type EntityType { get; set; }

        [DataMember]
        public JoinType JoinType { get; set; }

        [DataMember]
        public string JoinOn { get; set; }

        [DataMember]
        public Table JoinedTable { get; set; }

        [DataMember]
        public int index { get; set; }
        public bool ReverseRelation { get; set; }
        [DataMember]
        public List<Helpers.Table> Joins { get; set; }

        private Query.BaseQuery _query;

        public Query.BaseQuery Query
        {
            get
            {
                return this._query;
            }
        }

        //public static Table Get(BaseQuery query, Type entityType)
        //{
        //    return query.Data.Tables.Where(op => op.EntityType.IsAssignableFrom(entityType)).FirstOrDefault();
        //}

        public string GetPrimaryKeyName()
        {
            return this._query.Context.Connection.GetPrimaryKeyName(this.EntityType);
        }

        public string GetForeignKeyName()
        {
            return this._query.Context.Connection.FormatDataElement(this._query.Context.Connection.GetMappedFieldName(this.EntityType.Name + "ID"));
        }
        public string FormatFieldName(string field)
        {
            return this._query.Context.Connection.FormatDataElement(this._query.Context.Connection.GetMappedFieldName(field));
        }
        public string BuildJoinString()
        {
            var sb = new StringBuilder();
            switch (this.JoinType)
            {
                case JoinType.Inner:
                    sb.Append(" INNER JOIN ");
                    break;
                case JoinType.Left:
                    sb.Append(" LEFT JOIN ");
                    break;
                case JoinType.Right:
                    sb.Append(" RIGHT JOIN ");
                    break;
                case JoinType.LeftOuter:
                    sb.Append(" LEFT OUTER JOIN ");
                    break;
                case JoinType.RightOuter:
                    sb.Append(" RIGHT OUTER JOIN ");
                    break;
            }
            sb.Append(this.FullName);
            sb.Append(" ON ");
            if (this.JoinType != JoinType.Right && this.JoinType != JoinType.RightOuter)
            {
                sb.Append(this.Alias);
                sb.Append(".");
                if (!this.ReverseRelation)
                    sb.Append(this._query.Context.Connection.GetPrimaryKeyName(this.EntityType));
                else
                    sb.Append(this._query.Context.Connection.FormatDataElement(this._query.Context.Connection.GetMappedFieldName(this.JoinOn)));

                sb.Append(" = ");

                if (this.JoinedTable != null)
                    sb.Append(this.JoinedTable.Alias);
                else
                    sb.Append(this._query.Data.MainTable.Alias);
                sb.Append(".");
                if (!this.ReverseRelation)
                    sb.Append(this._query.Context.Connection.FormatDataElement(this._query.Context.Connection.GetMappedFieldName(this.JoinOn)));
                else
                    sb.Append(this.JoinedTable.GetPrimaryKeyName());
            }
            else
            {
                if (this.JoinedTable != null)
                    sb.Append(this.JoinedTable.Alias);
                else
                    sb.Append(this._query.Data.MainTable.Alias);
                sb.Append(".");
                sb.Append(this._query.Context.Connection.FormatDataElement(this._query.Context.Connection.GetMappedFieldName(this.JoinOn)));

                sb.Append(" = ");

                sb.Append(this.Alias);
                sb.Append(".");
                sb.Append(this._query.Context.Connection.GetPrimaryKeyName(this.EntityType));
            }
            return sb.ToString();
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {

        }
        internal Table AddJoin(Table table, params List<Table>[] rootJoins)
        {
            var existing = this.Joins.Where(op => op.JoinedTable != null && op.JoinedTable.Name == table.JoinedTable.Name && op.JoinOn == table.JoinOn && op.Name == table.Name).FirstOrDefault();
            if (existing == null)
            {
                if (rootJoins != null)
                {
                    foreach (var item in rootJoins)
                    {
                        existing = item.Where(op => op.JoinedTable != null && op.JoinedTable.Name == table.JoinedTable.Name && op.JoinOn == table.JoinOn && op.Name == table.Name).FirstOrDefault();
                        if (existing != null)
                            break;
                    }
                }
                if (existing == null)
                {
                    this.Joins.Add(table);
                    return table;
                }
            }
            return existing;
        }
        public Table(BaseQuery query, Type entityType) : this(query, entityType, "")
        {

        }
        public Table(BaseQuery query, Type entityType, JoinType joinType, string index = "0") : this(query, entityType, "J" + index)
        {
            this.JoinType = joinType;
        }
        public Table(BaseQuery query, Type entityType, string alias, int index = 0, string tableName = "", string schemaName = "")
        {
            this.Joins = new List<Helpers.Table>();
            this.index = index;
            this._query = query;
            this.EntityType = entityType;
            this.EntityTypeName = this.EntityType.FullName;

            if (string.IsNullOrEmpty(alias))
                this.Alias = "T0";// + query.Data.Tables.Count;
            else
                this.Alias = alias;
            if (this.EntityType != null)
                this.Name = query.Context.Connection.GetTableName(this.EntityType);
            else
                this.Name = query.Context.Connection.GetTableName(schemaName, tableName);

            this.FullName = this.Name + " " + this.Alias;
            if (this.Query.Context.Connection.Type == DatabaseType.SQLServer)
                this.FullName += " WITH (NOLOCK)";
        }
    }
}
