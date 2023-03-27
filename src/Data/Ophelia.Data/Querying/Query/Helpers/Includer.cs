using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Ophelia.Data.Querying.Query.Helpers
{
    [DataContract]
    public class Includer : Filter
    {
        [DataMember]
        public JoinType JoinType { get; set; }
        public PropertyInfo ReferencePropertyInfo { get; internal set; }
        public bool BuildAsXML { get; set; }

        [DataMember]
        public List<Includer> SubIncluders { get; set; }
        public static Includer Create(PropertyInfo info, string path, JoinType joinType)
        {
            if (info == null)
                throw new Exception("PropertyInfo not found. Path: " + path);

            var includer = new Includer();
            includer.Name = path;
            includer.PropertyInfo = info;
            includer.IsDataEntity = info.PropertyType.IsDataEntity() || info.PropertyType.IsPOCOEntity();
            includer.IsQueryableDataSet = info.PropertyType.IsQueryableDataSet() || typeof(System.Collections.IEnumerable).IsAssignableFrom(info.PropertyType) || info.PropertyType.IsQueryable();
            if (includer.IsQueryableDataSet)
                includer.EntityType = info.PropertyType.GetGenericArguments()[0];
            else if (includer.IsDataEntity)
                includer.EntityType = info.PropertyType;

            includer.JoinType = joinType;
            return includer.DecideType();
        }
        private Includer DecideType()
        {
            var idProperty = Extensions.GetForeignKeyProp(this.PropertyInfo);
            if (idProperty.Item1 != null)
            {
                this.ReferencePropertyInfo = idProperty.Item1;
                if (idProperty.Item1.PropertyType.IsNullable())
                    this.JoinType = JoinType.Left;
            }
            return this;
        }
        public static Includer Create(Expression expression, JoinType joinType)
        {
            return ExpressionParser.Create(expression).ToIncluder(joinType).DecideType();
        }

        public override string Build(Query.BaseQuery query, Table subqueryTable = null)
        {
            if (this.EntityType == null && !string.IsNullOrEmpty(this.EntityTypeName))
            {
                this.EntityType = this.EntityTypeName.ResolveType();
            }
            if (this.PropertyInfo == null && !string.IsNullOrEmpty(this.Name))
            {
                this.PropertyInfo = query.Data.MainTable.EntityType.GetPropertyInfo(this.Name);
            }
            var sb = new StringBuilder();
            if (this.EntityType == null && this.SubIncluders.Count > 0)
            {
                if (this.Take > 0)
                    this.SubIncluders.FirstOrDefault().Take = this.Take;
                if (this.Skip > 0)
                    this.SubIncluders.FirstOrDefault().Skip = this.Skip;
                return this.SubIncluders.FirstOrDefault().Build(query);
            }
            if (string.IsNullOrEmpty(this.Name) && this.EntityType == null && this.SubFilter != null)
            {
                this.SubIncluders.Add(this.SubFilter.ToIncluder());
                this.SubIncluders.FirstOrDefault().BuildAsXML = this.BuildAsXML;
                return this.SubIncluders.FirstOrDefault().Build(query, subqueryTable);
            }
            if (this.IsQueryableDataSet)
            {
                var index = (subqueryTable != null ? subqueryTable.index + 1 : query.Data.MainTable.index + 1);
                var subTable = new Helpers.Table(query, this.EntityType, "IN" + index, index);
                this.Table = subTable;

                sb.Append("(");
                sb.Append("SELECT ");
                if (this.Take > 0 && query.Context.Connection.Type == DatabaseType.SQLServer)
                {
                    sb.Append("TOP ");
                    sb.Append(this.Take);
                    sb.Append(" ");
                }
                if (query.Context.Connection.Type == DatabaseType.PostgreSQL)
                {
                    sb.Append("xmlagg(xmlelement(name ");
                    sb.Append(query.Context.Connection.FormatDataElement(query.Context.Connection.GetMappedFieldName(this.Name)));
                    sb.Append(",");
                }
                if (query.Context.Connection.Type == DatabaseType.Oracle)
                {
                    sb.Append("xmlagg(xmlelement(");
                    sb.Append(query.Context.Connection.FormatDataElement(query.Context.Connection.GetMappedFieldName(this.Name)));
                    sb.Append(",");
                }
                sb.Append(query.Context.Connection.GetAllSelectFields(subTable, true, true));
                if (this.SubIncluders.Count > 0)
                {
                    foreach (var item in this.SubIncluders)
                    {
                        item.BuildAsXML = true;
                        sb.Append(",");
                        sb.Append(item.Build(query, subTable));
                    }
                }
                if (query.Context.Connection.Type == DatabaseType.PostgreSQL || query.Context.Connection.Type == DatabaseType.Oracle)
                {
                    sb.Append("))");
                }
                var foreignKeyRelationAttribute = this.PropertyInfo.GetCustomAttributes(typeof(Attributes.RelationFKProperty)).FirstOrDefault() as Attributes.RelationFKProperty;
                var n2nRelationAttribute = this.PropertyInfo.GetCustomAttributes(typeof(Attributes.N2NRelationProperty)).FirstOrDefault() as Attributes.N2NRelationProperty;
                Helpers.Table relationTable = null;
                if (n2nRelationAttribute != null)
                {
                    relationTable = new Helpers.Table(query, n2nRelationAttribute.RelationClassType, "REL" + index, index);
                }

                sb.Append(" FROM ");
                sb.Append(subTable.FullName);
                if (relationTable != null)
                {
                    if (!string.IsNullOrEmpty(n2nRelationAttribute.ReverseFilterName))
                        relationTable.JoinOn = n2nRelationAttribute.ReverseFilterName;
                    else
                    {
                        relationTable.JoinOn = subTable.EntityType.Name + "ID";
                    }

                    relationTable.ReverseRelation = true;
                    relationTable.JoinType = JoinType.Inner;
                    relationTable.JoinedTable = subTable;
                    subTable.Joins.Add(relationTable);
                }
                if (subTable.Joins.Count > 0)
                {
                    sb.Append(" ");
                    foreach (var t in subTable.Joins)
                    {
                        sb.Append(t.BuildJoinString());
                    }
                }
                sb.Append(" WHERE ");
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

                if (n2nRelationAttribute == null)
                {
                    sb.Append(" = ");
                    sb.Append(subTable.Alias);
                    sb.Append(".");

                    if (foreignKeyRelationAttribute == null)
                    {
                        if (subqueryTable == null)
                            sb.Append(query.Data.MainTable.GetForeignKeyName(subTable.EntityType));
                        else
                            sb.Append(subqueryTable.GetForeignKeyName());
                    }
                    else
                    {
                        sb.Append(query.Data.MainTable.FormatFieldName(foreignKeyRelationAttribute.PropertyName));
                    }

                    var filterProperties = this.PropertyInfo.GetCustomAttributes(typeof(Attributes.RelationFilterProperty)).ToList();
                    if (filterProperties != null && filterProperties.Count > 0)
                    {
                        foreach (Attributes.RelationFilterProperty item in filterProperties)
                        {
                            sb.Append(" AND ");
                            sb.Append(subTable.Alias);
                            sb.Append(".");
                            sb.Append(query.Data.MainTable.FormatFieldName(item.PropertyName));
                            this.AddParameter(sb, query, item.Value, null, item.Comparison, false, this.IsStringProperty(subTable.EntityType.GetProperty(item.PropertyName), null));
                        }
                    }
                }
                else
                {
                    sb.Append(" = ");
                    sb.Append(relationTable.Alias);
                    sb.Append(".");
                    if (!string.IsNullOrEmpty(n2nRelationAttribute.FilterName))
                        sb.Append(relationTable.FormatFieldName(query.Context.Connection.GetMappedFieldName(n2nRelationAttribute.FilterName)));
                    else
                    {
                        if (subqueryTable == null)
                            sb.Append(query.Data.MainTable.GetForeignKeyName());
                        else
                            sb.Append(subqueryTable.GetForeignKeyName());
                    }
                }

                if (this.SubFilter != null)
                {
                    var filterString = this.SubFilter.Build(query, subTable);
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        sb.Append(" AND ");
                        sb.Append(filterString);
                    }
                }
                if (this.Take > 0 && (query.Context.Connection.Type == DatabaseType.PostgreSQL || query.Context.Connection.Type == DatabaseType.Oracle))
                {
                    sb.Append("LIMIT ");
                    sb.Append(this.Take);
                    sb.Append(" ");
                }
                if (query.Context.Connection.Type == DatabaseType.SQLServer)
                {
                    sb.Append(" FOR XML PATH) AS ");
                }
                else
                {
                    sb.Append(")  AS ");
                }
                sb.Append(query.Data.MainTable.FormatFieldName(this.Name));
            }
            else if (this.IsDataEntity)
            {
                var table = query.Data.MainTable;
                var joinType = this.JoinType;
                var hasParentQuery = false;
                if (subqueryTable != null)
                {
                    table = subqueryTable;
                    joinType = JoinType.Left;
                    hasParentQuery = true;
                }
                if (hasParentQuery)
                    this.Table = table.AddJoin(new Table(query, this.EntityType, joinType, table.Joins.Count + query.GetTableJoinIndex()) { JoinOn = query.Context.Connection.GetMappedFieldName(Extensions.GetForeignKeyName(this.PropertyInfo)), JoinedTable = table }, table.Joins);
                else
                    this.Table = table.AddJoin(new Table(query, this.EntityType, joinType, table.Joins.Count + query.GetTableJoinIndex()) { JoinOn = query.Context.Connection.GetMappedFieldName(Extensions.GetForeignKeyName(this.PropertyInfo)), JoinedTable = table }, table.Joins, query.Data.MainTable.Joins);
                this.Tables.Add(this.Table);

                var selectedMembers = new List<PropertyInfo>();
                foreach (var selector in query.Data.Selectors)
                {
                    // TODO : If selector members doesn't contains an included field (like selecting an id list) all fields are being selected.It may be a performance problem.
                    if (selector.Members != null)
                    {
                        foreach (PropertyInfo member in selector.Members)
                        {
                            if (member.DeclaringType == this.PropertyInfo.PropertyType)
                                selectedMembers.Add(member);
                        }
                    }
                }
                if (!selectedMembers.Any())
                    sb.Append(query.Context.Connection.GetAllSelectFields(this.Table, true, this.BuildAsXML));
                else
                {
                    foreach (var member in selectedMembers)
                    {
                        sb.Append(query.Context.Connection.GetFieldSelectString(this.Table, member, true, this.BuildAsXML));
                        if (member != selectedMembers.LastOrDefault())
                            sb.Append(",");
                    }
                }
                if (this.SubIncluders.Count > 0)
                {
                    foreach (var item in this.SubIncluders)
                    {
                        item.BuildAsXML = this.BuildAsXML;
                        sb.Append(",");
                        sb.Append(item.Build(query, this.Table));
                    }
                }
                foreach (var join in this.Table.Joins)
                {
                    if (this.SubIncluders.Count == 0)
                        table.AddJoin(join);
                    else
                        table.AddJoinWithoutCheck(join);
                }
            }
            else
            {
                throw new Exception("Primitive types can not be included");
            }
            return sb.ToString();
        }

        internal void SetReferencedEntities(BaseQuery query, System.Data.DataRow row, object entity)
        {
            if (this.EntityType == null && this.SubIncluders != null)
            {
                foreach (var item in this.SubIncluders)
                {
                    item.SetReferencedEntities(query, row, entity);
                }
            }
            if (this.IsDataEntity)
            {
                if (this.Table == null)
                    return;

                var baseName = query.Context.Connection.GetMappedFieldName(this.Table.Alias + "_");
                var found = false;
                foreach (System.Data.DataColumn item in row.Table.Columns)
                {
                    if (item.ColumnName.StartsWith(baseName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    return;

                object referencedEntity = null;
                var properties = this.PropertyInfo.PropertyType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(op => !op.PropertyType.IsDataEntity() && !op.PropertyType.IsQueryableDataSet());
                foreach (var p in properties)
                {
                    var fieldName = query.Context.Connection.CheckCharLimit(query.Context.Connection.GetMappedFieldName(baseName + Extensions.GetColumnName(p)));
                    if (row.Table.Columns.Contains(fieldName) && row[fieldName] != DBNull.Value)
                    {
                        if (referencedEntity == null)
                        {
                            referencedEntity = Activator.CreateInstance(this.PropertyInfo.PropertyType);
                            if (referencedEntity is Model.DataEntity)
                                (referencedEntity as Model.DataEntity).InternalTracker.State = EntityState.Loading;
                        }
                        if (p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                        {
                            var value = row[fieldName];
                            if (value == null)
                            {
                                p.SetValue(referencedEntity, value);
                            }
                            else
                            {
                                p.SetValue(referencedEntity, Nullable.GetUnderlyingType(p.PropertyType).ConvertData(value));
                            }
                        }
                        else
                            p.SetValue(referencedEntity, p.PropertyType.ConvertData(row[fieldName]));
                    }
                }
                if (referencedEntity != null && this.PropertyInfo.DeclaringType == entity.GetType())
                {
                    var pkProp = Extensions.GetPrimaryKeyProperty(referencedEntity.GetType());
                    if (Convert.ToInt64(pkProp.GetValue(referencedEntity)) > 0)
                    {
                        this.PropertyInfo.SetValue(entity, referencedEntity);
                        if (referencedEntity is Model.DataEntity)
                            (referencedEntity as Model.DataEntity).InternalTracker.State = EntityState.Loaded;

                        foreach (var subInc in this.SubIncluders)
                        {
                            subInc.SetReferencedEntities(query, row, referencedEntity);
                        }
                    }
                }
            }
            else if (this.IsQueryableDataSet)
            {
                if (this.Table == null)
                    return;

                var refFieldName = query.Context.Connection.CheckCharLimit(query.Context.Connection.GetMappedFieldName(this.Name));
                if (!row.Table.Columns.Contains(refFieldName) || row[refFieldName] == DBNull.Value)
                    return;

                var types = new Type[] { this.EntityType };

                if (entity.GetType().IsDataEntity())
                {
                    (entity as Model.DataEntity).InternalTracker.LoadAnyway = true;
                }
                IEnumerable referencedCollection = null;
                if (this.PropertyInfo.PropertyType.IsQueryableDataSet() || this.PropertyInfo.PropertyType.IsQueryable())
                {
                    referencedCollection = (IEnumerable)this.PropertyInfo.GetValue(entity);
                }
                if (referencedCollection == null)
                {
                    referencedCollection = this.PropertyInfo.PropertyType.GenericTypeArguments[0].CreateList();
                    this.PropertyInfo.SetValue(entity, referencedCollection);
                }
                if (entity.GetType().IsDataEntity())
                {
                    (entity as Model.DataEntity).InternalTracker.LoadAnyway = false;
                }

                var doc = new System.Xml.XmlDocument();
                doc.LoadXml("<rows>" + row[refFieldName].ToString() + "</rows>");
                var xmlReader = new System.Xml.XmlNodeReader(doc);
                var dataSet = new System.Data.DataSet();
                dataSet.ReadXml(xmlReader);

                if (dataSet.Tables.Count > 0)
                {
                    object referencedEntity = null;

                    if (referencedCollection.GetType().IsQueryableDataSet())
                        (referencedCollection as Model.QueryableDataSet).TotalCount = dataSet.Tables[0].Rows.Count;

                    foreach (System.Data.DataRow item in dataSet.Tables[0].Rows)
                    {
                        referencedEntity = Activator.CreateInstance(this.EntityType);
                        if (referencedCollection.GetType().IsQueryableDataSet())
                            (referencedCollection as Model.QueryableDataSet).AddItem(referencedEntity);
                        else
                        {
                            referencedCollection.ExecuteMethod("Add", referencedEntity);
                        }

                        if (referencedEntity.GetType().IsDataEntity())
                            (referencedEntity as Model.DataEntity).InternalTracker.State = EntityState.Loading;

                        var properties = this.EntityType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(op => !op.PropertyType.IsDataEntity() && !op.PropertyType.IsQueryableDataSet());
                        foreach (var p in properties)
                        {
                            var fieldName = query.Context.Connection.CheckCharLimit(query.Context.Connection.GetMappedFieldName(this.Table.Alias + "_" + Extensions.GetColumnName(p)));
                            if (item.Table.Columns.Contains(fieldName) && item[fieldName] != DBNull.Value)
                            {
                                if (item[fieldName] != DBNull.Value)
                                {
                                    try
                                    {
                                        p.SetValue(referencedEntity, p.PropertyType.ConvertData(item[fieldName]));
                                    }
                                    catch (Exception)
                                    {
                                        continue;
                                    }
                                }
                            }
                        }

                        foreach (var subInc in this.SubIncluders)
                        {
                            subInc.SetReferencedEntities(query, item, referencedEntity);
                        }

                        if (referencedEntity is Model.DataEntity)
                            (referencedEntity as Model.DataEntity).InternalTracker.State = EntityState.Loaded;
                    }
                }
            }
        }

        public Includer()
        {
            this.SubIncluders = new List<Includer>();
        }
        public new Includer Serialize()
        {
            var entity = base.Serialize() as Includer;
            entity.Name = this.Name;
            entity.JoinType = this.JoinType;
            if (this.SubIncluders != null)
            {
                foreach (var item in this.SubIncluders)
                {
                    entity.SubIncluders.Add(item.Serialize());
                }
            }
            return entity;
        }
    }
}
