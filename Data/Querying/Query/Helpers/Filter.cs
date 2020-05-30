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
    public class Filter : IDisposable
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public Filter Left { get; set; }

        [DataMember]
        public Filter Right { get; set; }

        [DataMember]
        public Constraint Constraint { get; set; }

        [DataMember]
        public Comparison Comparison { get; set; }

        [XmlIgnore]
        public Filter ParentFilter { get; set; }

        [DataMember]
        public Filter SubFilter { get; set; }

        [XmlIgnore]
        public PropertyInfo PropertyInfo { get; set; }

        [DataMember]
        public bool Exclude { get; set; }

        [DataMember]
        public object Value { get; set; }

        [DataMember]
        public string ValueType { get; set; }

        [DataMember]
        public object Value2 { get; set; }

        [DataMember]
        public string Value2Type { get; set; }

        [DataMember]
        public int Take { get; set; }

        [DataMember]
        public int Skip { get; set; }

        [DataMember]
        public bool IsDataEntity { get; set; }

        [DataMember]
        public bool IsQueryableDataSet { get; set; }

        [DataMember]
        public string EntityTypeName { get; set; }

        [XmlIgnore]
        public Type EntityType { get; set; }

        [DataMember]
        public Query.Helpers.Table Table { get; set; }

        [DataMember]
        public List<Query.Helpers.Table> Tables { get; set; }

        public Filter()
        {
            this.Tables = new List<Table>();
        }

        public static Filter Create(Expression expression)
        {
            return ExpressionParser.Create(expression).ToFilter();
        }
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.Tables.Clear();
            this.Tables = null;
            if (this.SubFilter != null)
            {
                this.SubFilter.Dispose();
                this.SubFilter = null;
            }
            if (this.Left != null)
            {
                this.Left.Dispose();
                this.Left = null;
            }
            if (this.Right != null)
            {
                this.Right.Dispose();
                this.Right = null;
            }
            this.ParentFilter = null;
            this.Table = null;
            this.EntityType = null;
            this.Value = null;
            this.Value2 = null;
        }

        public virtual string Build(Query.BaseQuery query, Table subqueryTable = null)
        {
            if (string.IsNullOrEmpty(this.Name) && this.SubFilter != null)
            {
                this.SubFilter.Exclude = this.Exclude;
                return this.SubFilter.Build(query, subqueryTable);
            }
            else if (string.IsNullOrEmpty(this.Name) && this.Left == null && this.Right == null && !this.IsQueryableDataSet)
            {
                return "";
            }
            var isStringFilter = false;
            var sb = new StringBuilder();
            if (this.Left != null && this.Right != null)
            {
                var leftStr = this.Left.Build(query, subqueryTable);
                var rightStr = this.Right.Build(query, subqueryTable);

                if (!string.IsNullOrEmpty(leftStr) || !string.IsNullOrEmpty(rightStr))
                {
                    sb.Append("(");
                    sb.Append(leftStr);
                    if (!string.IsNullOrEmpty(leftStr))
                    {
                        if (this.Constraint == Constraint.And)
                            sb.Append(" AND ");
                        else
                            sb.Append(" OR ");
                    }
                    sb.Append(rightStr);
                    sb.Append(")");
                }
            }
            else
            {
                if (this.Exclude && this.Comparison != Comparison.Equal)
                    sb.Append(" NOT ");

                if (this.Comparison == Comparison.Exists)
                {
                    //Collection filtering
                    Type entityType = null;
                    if (this.Value is Model.QueryableDataSet)
                    {
                        entityType = this.EntityType;
                    }
                    else
                    {
                        entityType = (this.Value as Type).GetGenericArguments()[0];
                    }

                    var index = (subqueryTable != null ? subqueryTable.index + 1 : query.Data.MainTable.index + 1);
                    var subTable = new Table(query, entityType, "SQ" + index, index);
                    this.Tables.Add(subTable);

                    sb.Append("EXISTS (");
                    sb.Append("SELECT NULL FROM " + subTable.FullName);
                    sb.Append(" WHERE ");
                    if (subqueryTable == null && !string.IsNullOrEmpty(this.Name) && this.Name.IndexOf(".") > -1)
                    {
                        subqueryTable = this.FindTable(query, this.ParentFilter.PropertyInfo);
                    }
                    if (subqueryTable == null)
                    {
                        sb.Append(query.Data.MainTable.Alias);
                        sb.Append(".");
                        sb.Append(query.Data.MainTable.GetPrimaryKeyName());
                    }
                    else
                    {
                        sb.Append(subqueryTable.Alias);
                        sb.Append(".");
                        sb.Append(subqueryTable.GetPrimaryKeyName());
                    }
                    sb.Append(" = ");
                    sb.Append(subTable.Alias);
                    sb.Append(".");
                    if (subqueryTable == null)
                        sb.Append(query.Data.MainTable.GetForeignKeyName());
                    else
                        sb.Append(subqueryTable.GetForeignKeyName());

                    if (this.Value2 is Attributes.N2NRelationProperty)
                    {
                        var relation = this.Value2 as Attributes.N2NRelationProperty;
                        sb.Append(" AND ");
                        sb.Append(subTable.Alias);
                        sb.Append(".");
                        sb.Append(query.Context.Connection.FormatDataElement(query.Context.Connection.GetMappedFieldName(relation.FilterName)));
                        sb.Append("=");
                        sb.Append(query.Context.Connection.FormatParameterName("p") + query.Data.Parameters.Count);
                        query.Data.Parameters.Add(query.Context.Connection.FormatParameterValue(relation.FilterValue));
                    }
                    if (this.SubFilter != null)
                    {
                        sb.Append(" AND ");
                        sb.Append(this.SubFilter.Build(query, subTable));
                    }
                    else
                    {

                    }
                    sb.Append(")");
                }
                else if (this.Name.IndexOf(".") == -1)
                {
                    isStringFilter = this.IsStringProperty(this.PropertyInfo, this.Value);
                    if (query.Context.Connection.Type == DatabaseType.Oracle && isStringFilter)
                        sb.Append("UPPER(");

                    if (subqueryTable == null)
                        sb.Append(query.Data.MainTable.Alias);
                    else
                        sb.Append(subqueryTable.Alias);
                    sb.Append(".");
                    sb.Append(query.Context.Connection.FormatDataElement(query.Context.Connection.GetMappedFieldName(this.Name)));

                    if (query.Context.Connection.Type == DatabaseType.Oracle && isStringFilter)
                        sb.Append(")");
                }
                else
                {
                    var name = "";
                    //var props = this.Name.Split('.');
                    Type lastType = query.Data.EntityType;
                    var props = query.Data.EntityType.GetPropertyInfoTree(this.Name);
                    isStringFilter = false;
                    foreach (var _prop in props)
                    {
                        lastType = _prop.PropertyType;
                        if (lastType.IsDataEntity())
                        {
                            var table = query.Data.MainTable;
                            if (subqueryTable != null)
                                table = subqueryTable;

                            Table joinedTable = null;
                            var propInfo = lastType.GetProperties().Where(op => op.Name.Equals(_prop.Name + "ID", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                            if (propInfo != null && Nullable.GetUnderlyingType(propInfo.PropertyType) != null)
                            {
                                joinedTable = table.Joins.Where(op => op.JoinOn == propInfo.Name).FirstOrDefault();
                                if (joinedTable == null)
                                {
                                    var toJoinTable = table.Joins.LastOrDefault();
                                    if (toJoinTable == null)
                                        toJoinTable = query.Data.MainTable;
                                    else if (toJoinTable.EntityType.GetProperty(propInfo.Name) == null)
                                        toJoinTable = query.Data.MainTable;

                                    joinedTable = table.AddJoin(new Table(query, lastType, JoinType.Left, table.Joins.Count.ToString()) { JoinOn = query.Context.Connection.GetMappedFieldName(propInfo.Name), JoinedTable = toJoinTable });
                                }
                            }
                            else
                            {
                                joinedTable = table.Joins.Where(op => op.JoinOn == _prop.Name + "ID").FirstOrDefault();
                                if (joinedTable == null)
                                {
                                    var toJoinTable = table.Joins.LastOrDefault();
                                    if (toJoinTable == null)
                                        toJoinTable = query.Data.MainTable;
                                    else if (toJoinTable.EntityType.GetProperty(_prop.Name + "ID") == null)
                                        toJoinTable = query.Data.MainTable;

                                    joinedTable = table.AddJoin(new Table(query, lastType, JoinType.Left, table.Joins.Count.ToString()) { JoinOn = query.Context.Connection.GetMappedFieldName(_prop.Name + "ID"), JoinedTable = toJoinTable });
                                }
                            }

                            if (!this.Tables.Where(op => op.Alias == joinedTable.Alias).Any())
                                this.Tables.Add(joinedTable);

                            if (_prop.Equals(props[props.Length - 2]))
                            {
                                isStringFilter = this.IsStringProperty(props.LastOrDefault(), null);
                                if (query.Context.Connection.Type == DatabaseType.Oracle && isStringFilter)
                                    name += "UPPER(";

                                name += joinedTable.Alias;
                                name += ".";
                            }
                            if (_prop == props.LastOrDefault())
                                name += query.Context.Connection.GetPrimaryKeyName(lastType);
                        }
                        else if (lastType.IsQueryableDataSet())
                        {

                        }
                        else
                        {
                            name += query.Context.Connection.FormatDataElement(query.Context.Connection.GetMappedFieldName(_prop.Name));

                            if (query.Context.Connection.Type == DatabaseType.Oracle && isStringFilter)
                                name += ")";
                        }
                    }
                    sb.Append(name);
                }

                var oracleNull = false;
                if (query.Context.Connection.Type == DatabaseType.Oracle)
                {
                    if (this.Comparison != Comparison.Exists && this.Comparison != Comparison.In && this.Comparison != Comparison.Between)
                    {
                        if (this.Value != null && this.Value.GetType().Name == "String" && String.IsNullOrEmpty(Convert.ToString(this.Value)))
                        {
                            oracleNull = true;
                        }
                    }
                }
                switch (this.Comparison)
                {
                    case Comparison.Equal:
                        if (this.Value == null)
                        {
                            if (this.Exclude)
                                sb.Append(" IS NOT NULL ");
                            else
                                sb.Append(" IS NULL ");
                        }
                        else
                        {
                            if (this.Exclude)
                            {
                                if (query.Context.Connection.Type == DatabaseType.Oracle)
                                    sb.Append(" != ");
                                else
                                    sb.Append(" <> ");
                            }
                            else
                                sb.Append(" = ");

                            if (!oracleNull)
                                sb.Append(query.Context.Connection.FormatParameterName("p") + query.Data.Parameters.Count);
                            else
                                sb.Append("''");
                        }
                        break;
                    case Comparison.Different:
                        if (this.Value == null)
                        {
                            if (this.Exclude)
                                sb.Append(" IS NULL ");
                            else
                            {
                                sb.Append(" IS NOT NULL ");
                            }
                        }
                        else
                        {
                            if (this.Exclude)
                                sb.Append(" = ");
                            else
                            {
                                if (query.Context.Connection.Type == DatabaseType.Oracle)
                                    sb.Append(" != ");
                                else
                                    sb.Append(" <> ");
                            }
                            if (!oracleNull)
                                sb.Append(query.Context.Connection.FormatParameterName("p") + query.Data.Parameters.Count);
                            else
                                sb.Append("''");
                        }
                        break;
                    case Comparison.Greater:
                        sb.Append(" > ");
                        sb.Append(query.Context.Connection.FormatParameterName("p") + query.Data.Parameters.Count);
                        break;
                    case Comparison.Less:
                        sb.Append(" < ");
                        sb.Append(query.Context.Connection.FormatParameterName("p") + query.Data.Parameters.Count);
                        break;
                    case Comparison.GreaterAndEqual:
                        sb.Append(" >= ");
                        sb.Append(query.Context.Connection.FormatParameterName("p") + query.Data.Parameters.Count);
                        break;
                    case Comparison.LessAndEqual:
                        sb.Append(" <= ");
                        sb.Append(query.Context.Connection.FormatParameterName("p") + query.Data.Parameters.Count);
                        break;
                    case Comparison.In:
                        sb.Append(" IN (");
                        if (this.Value.GetType().IsEnumarable())
                        {
                            var enumarable = this.Value as System.Collections.IEnumerable;
                            var itemsSQL = "";
                            foreach (object item in enumarable)
                            {
                                if (!string.IsNullOrEmpty(itemsSQL))
                                    itemsSQL += ",";

                                itemsSQL += item.ToString();
                            }
                            if (string.IsNullOrEmpty(itemsSQL))
                                itemsSQL = "'0'";
                            sb.Append(itemsSQL);
                        }
                        else
                            sb.Append(this.Value.ToString());
                        sb.Append(")");
                        break;
                    case Comparison.Between:
                        sb.Append(" BETWEEN ");
                        sb.Append(query.Context.Connection.FormatParameterName("p") + query.Data.Parameters.Count);
                        query.Data.Parameters.Add(query.Context.Connection.FormatParameterValue(this.Value));
                        sb.Append(" AND ");
                        sb.Append(query.Context.Connection.FormatParameterName("p") + query.Data.Parameters.Count);
                        query.Data.Parameters.Add(query.Context.Connection.FormatParameterValue(this.Value2));
                        break;
                    case Comparison.StartsWith:
                        if (query.Context.Connection.Type == DatabaseType.PostgreSQL)
                            sb.Append(query.Context.Connection.FormatStringConcat(" ILIKE "));
                        else
                            sb.Append(query.Context.Connection.FormatStringConcat(" LIKE "));
                        if (!oracleNull)
                        {
                            sb.Append(query.Context.Connection.FormatParameterName("p") + query.Data.Parameters.Count);
                            sb.Append(query.Context.Connection.FormatStringConcat("+ '%'"));
                        }
                        else
                            sb.Append(query.Context.Connection.FormatStringConcat("'%'"));
                        break;
                    case Comparison.EndsWith:
                        if (query.Context.Connection.Type == DatabaseType.PostgreSQL)
                            sb.Append(query.Context.Connection.FormatStringConcat(" ILIKE "));
                        else
                            sb.Append(query.Context.Connection.FormatStringConcat(" LIKE "));
                        if (!oracleNull)
                        {
                            sb.Append(query.Context.Connection.FormatStringConcat("'%' + "));
                            sb.Append(query.Context.Connection.FormatParameterName("p") + query.Data.Parameters.Count);
                        }
                        else
                            sb.Append(query.Context.Connection.FormatStringConcat("'%'"));
                        break;
                    case Comparison.Contains:
                        if (query.Context.Connection.Type == DatabaseType.PostgreSQL)
                            sb.Append(query.Context.Connection.FormatStringConcat(" ILIKE '%' + "));
                        else if (query.Context.Connection.Type == DatabaseType.Oracle)
                        {
                            if (!oracleNull)
                                sb.Append(query.Context.Connection.FormatStringConcat(" LIKE '%' + "));
                            else
                                sb.Append(query.Context.Connection.FormatStringConcat(" LIKE '%'"));
                        }
                        else
                            sb.Append(query.Context.Connection.FormatStringConcat(" LIKE '%' + "));

                        if (!oracleNull)
                        {
                            sb.Append(query.Context.Connection.FormatParameterName("p") + query.Data.Parameters.Count);
                            sb.Append(query.Context.Connection.FormatStringConcat(" + '%'"));
                        }
                        break;
                }
                if (!oracleNull && this.Comparison != Comparison.Exists && this.Comparison != Comparison.In && this.Comparison != Comparison.Between)
                {
                    if (this.Value != null)
                    {
                        if (isStringFilter && query.Context.Connection.Type == DatabaseType.Oracle)
                            query.Data.Parameters.Add(query.Context.Connection.FormatParameterValue(Convert.ToString(this.Value).ToUpper()));
                        else
                            query.Data.Parameters.Add(query.Context.Connection.FormatParameterValue(this.Value, isStringFilter));
                    }
                }
            }
            return sb.ToString();
        }
        private bool IsStringProperty(PropertyInfo info, object value)
        {
            if (info != null && info.PropertyType.Name == "String")
                return true;
            else if (value != null && value.GetType().Name == "String")
                return true;
            return false;
        }
        internal Table FindTable(Query.BaseQuery query, PropertyInfo info)
        {
            if (info == null)
                return null;

            if (this.Tables.Where(op => op.EntityType == info.PropertyType).Any())
                return this.Tables.Where(op => op.EntityType == info.PropertyType).FirstOrDefault();

            if (this.ParentFilter != null)
                return this.ParentFilter.FindTable(query, info);

            if (query.Data.MainTable.Joins.Where(op => op.EntityType == info.PropertyType).Any())
                return query.Data.MainTable.Joins.Where(op => op.EntityType == info.PropertyType).FirstOrDefault();

            return null;
        }
        public Includer ToIncluder()
        {
            var includer = new Includer();
            includer.Name = this.Name;
            includer.Constraint = this.Constraint;
            includer.Comparison = this.Comparison;
            includer.SubFilter = this.SubFilter;
            includer.Left = this.Left;
            includer.Right = this.Right;
            includer.Exclude = this.Exclude;
            includer.Value = this.Value;
            includer.Value2 = this.Value2;
            includer.Take = this.Take;
            includer.Skip = this.Skip;
            includer.IsDataEntity = this.IsDataEntity;
            includer.IsQueryableDataSet = this.IsQueryableDataSet;
            includer.EntityType = this.EntityType;
            includer.EntityTypeName = this.EntityTypeName;
            includer.Table = this.Table;
            return includer;
        }
        public Filter Serialize()
        {
            Filter entity = null;
            if (this.GetType().Name == "Filter")
                entity = new Filter();
            else
                entity = new Includer();

            entity.Name = this.Name;
            entity.Constraint = this.Constraint;
            entity.Comparison = this.Comparison;
            entity.Exclude = this.Exclude;
            entity.Value = this.Value;
            entity.Value2 = this.Value2;
            entity.ValueType = this.ValueType;
            entity.Value2Type = this.Value2Type;
            entity.Take = this.Take;
            entity.Skip = this.Skip;
            entity.IsDataEntity = this.IsDataEntity;
            entity.EntityTypeName = this.EntityTypeName;
            entity.IsQueryableDataSet = this.IsQueryableDataSet;
            if (this.SubFilter != null)
                entity.SubFilter = this.SubFilter.Serialize();
            if (this.Left != null)
                entity.Left = this.Left.Serialize();
            if (this.Right != null)
                entity.Right = this.Right.Serialize();
            return entity;
        }

        public object ProcessValue(object val, string type)
        {
            if (val != null)
            {
                bool isList = type.IndexOf("List") > -1;
                bool isArray = type.IndexOf("Array") > -1;

                var subType = type.Right(type.Length - type.IndexOf(",") - 1);
                Type dataType = null;
                switch (subType)
                {
                    case "Int64":
                        dataType = typeof(Int64);
                        break;
                    case "Int32":
                        dataType = typeof(Int32);
                        break;
                    case "Int16":
                        dataType = typeof(Int16);
                        break;
                    case "DateTime":
                        dataType = typeof(DateTime);
                        break;
                    default:
                        dataType = typeof(string);
                        break;
                }
                if (isList)
                {
                    if (val is Newtonsoft.Json.Linq.JArray)
                    {
                        var arr = val as Newtonsoft.Json.Linq.JArray;
                        var listType = typeof(List<>).MakeGenericType(dataType);
                        var list = (System.Collections.IList)Activator.CreateInstance(listType);
                        foreach (var item in arr)
                        {
                            list.Add(dataType.ConvertData(item.ToString()));
                        }
                        return list;
                    }
                }
                else if (isArray)
                {

                }
                try
                {
                    return dataType.ConvertData(val);
                }
                catch (Exception)
                {
                    return val;
                }
            }
            return null;
        }
    }
}
