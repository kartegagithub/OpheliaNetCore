﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Ophelia.Data.Querying.Query.Helpers
{
    [DataContract]
    public class Selector : IDisposable
    {
        [DataMember]
        public string Name { get; set; }

        public PropertyInfo PropertyInfo { get; set; }
        public List<MemberInfo> Members { get; set; }
        public Dictionary<MemberInfo, Expression> BindingMembers { get; set; }
        [DataMember]
        public Selector SubSelector { get; set; }

        [DataMember]
        public List<Query.Helpers.Table> Tables { get; set; }

        public static Selector Create(Expression expression)
        {
            return ExpressionParser.Create(expression).ToSelector();
        }
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {

        }

        public string Build(BaseQuery query)
        {
            this.Tables = new List<Table>();
            if (!string.IsNullOrEmpty(this.Name))
            {
                return this.Build(query, this.Name, this.PropertyInfo);
            }
            else if (this.Members != null && this.Members.Count > 0)
            {
                var sb = new StringBuilder();
                var counter = 0;
                foreach (var item in this.Members)
                {
                    if (counter > 0)
                        sb.Append(",");

                    if (this.BindingMembers != null && this.BindingMembers.Any())
                    {
                        var bindingMember = this.BindingMembers.ElementAt(counter);
                        if (bindingMember.Value is MemberExpression)
                            sb.Append(this.Build(query, bindingMember.Value.ParsePath(), (bindingMember.Value as MemberExpression).Member as PropertyInfo));
                        else if (bindingMember.Value is MemberInitExpression)
                            sb.Append(this.Build(query, bindingMember.Value as MemberInitExpression));
                    }
                    else
                        sb.Append(this.Build(query, item.Name, item as PropertyInfo));
                    counter++;
                }
                return sb.ToString().Replace(",,", ",");
            }
            else if (this.SubSelector != null)
                return this.SubSelector.Build(query);
            return "";
        }
        private string Build(BaseQuery query, MemberInitExpression expression)
        {
            var sb = new StringBuilder();
            var counter = 0;
            foreach (var bindingMember in expression.Bindings)
            {
                if (counter > 0)
                    sb.Append(",");
                if (bindingMember.BindingType == MemberBindingType.Assignment)
                {
                    if ((bindingMember as MemberAssignment).Expression is MemberExpression)
                        sb.Append(this.Build(query, (bindingMember as MemberAssignment).Expression.ParsePath(), ((bindingMember as MemberAssignment).Expression as MemberExpression).Member as PropertyInfo));
                    else if ((bindingMember as MemberAssignment).Expression is MemberInitExpression)
                        sb.Append(this.Build(query, (bindingMember as MemberAssignment).Expression as MemberInitExpression));
                }
                counter++;
            }
            return sb.ToString();
        }
        private string Build(BaseQuery query, string name, PropertyInfo propInfo)
        {
            if (propInfo.ReflectedType.IsAnonymousType() && name.Contains('.'))
                name = name.Right(name.Length - name.IndexOf('.') - 1);
            if (name.Contains('.'))
            {
                var props = query.Data.EntityType.GetPropertyInfoTree(name);
                var p = props[^2];
                var table = query.Data.MainTable.Joins.Where(op => op.JoinOn == Extensions.GetForeignKeyName(p) && op.EntityType == p.DeclaringType).FirstOrDefault();
                if (table != null)
                {
                    this.Tables.Add(table);
                    return query.Context.Connection.GetFieldSelectString(table, props.LastOrDefault(), true);
                }
            }
            else if (propInfo != null && propInfo.PropertyType.IsPrimitiveType())
                return query.Context.Connection.GetFieldSelectString(query.Data.MainTable, propInfo, false);
            else if (propInfo != null && (propInfo.PropertyType.IsDataEntity() || propInfo.PropertyType.IsPOCOEntity()))
            {
                var table = query.Data.MainTable.Joins.Where(op => op.JoinOn == Extensions.GetForeignKeyName(propInfo) && op.EntityType == propInfo.DeclaringType).FirstOrDefault();
                if (table != null)
                {
                    this.Tables.Add(table);
                    return query.Context.Connection.GetAllSelectFields(table, true);
                }
            }
            else if (propInfo == null)
                return query.Data.MainTable.Alias + "." + query.Context.Connection.FormatDataElement(query.Context.Connection.GetMappedFieldName(name));

            return "";
        }
        public void SetData(BaseQuery query, object entity, MemberInfo member, MemberInitExpression expression, Type type, System.Data.DataRow row)
        {
            var refEntity = expression.NewExpression.Constructor.Invoke(null);
            var p = type.GetProperty(member.Name);
            p.SetValue(entity, refEntity);
            foreach (var bindingMember in expression.Bindings)
            {
                if (bindingMember.BindingType == MemberBindingType.Assignment)
                {
                    var assignment = (bindingMember as MemberAssignment);
                    if (assignment != null)
                    {
                        if (assignment.Expression is MemberExpression)
                        {
                            this.SetData(bindingMember.Member, this.GetFieldName(query, bindingMember.Member, assignment.Expression as MemberExpression), row, refEntity.GetType(), refEntity, new KeyValuePair<MemberInfo, Expression>(bindingMember.Member, (bindingMember as MemberAssignment).Expression));
                        }
                        else if (assignment.Expression is MemberInitExpression)
                        {
                            this.SetData(query, refEntity, bindingMember.Member, assignment.Expression as MemberInitExpression, refEntity.GetType(), row);
                        }
                    }
                }
            }
        }
        public string GetFieldName(BaseQuery query, MemberInfo member, Expression expression)
        {
            var memberName = Extensions.GetColumnName(member);
            var path = expression.ParsePath();
            if (path.IndexOf(".") > -1)
            {
                var props = query.Data.EntityType.GetPropertyInfoTree(path);
                var p = props[props.Length - 2];
                var table = query.Data.MainTable.Joins.Where(op => op.JoinOn == Extensions.GetForeignKeyName(p) && op.EntityType == p.DeclaringType).FirstOrDefault();
                if (table != null)
                    return query.Context.Connection.GetMappedFieldName(table.Alias + "_" + memberName);
            }
            else
                return query.Context.Connection.GetMappedFieldName(memberName);

            return "";
        }
        public void SetData(BaseQuery query, object entity, Type type, System.Data.DataRow row)
        {
            var counter = 0;
            foreach (PropertyInfo member in this.Members)
            {
                var fieldName = "";
                var bindingMember = new KeyValuePair<MemberInfo, Expression>();
                if (this.BindingMembers != null && this.BindingMembers.Any())
                    bindingMember = this.BindingMembers.ElementAt(counter);

                if (this.Tables == null || !this.Tables.Any())
                {
                    if (member.GetMemberInfoType().IsQueryable())
                    {
                        foreach (var includer in query.Data.Includers)
                        {
                            if (query.IncluderIsSelected(includer))
                                includer.SetReferencedEntities(query, row, entity);
                        }
                        counter++;
                        continue;
                    }
                    else
                    {
                        foreach (var includer in query.Data.Includers)
                        {
                            if (includer.PropertyInfo != null && includer.PropertyInfo.PropertyType == member.DeclaringType)
                            {
                                if (includer.Table != null)
                                {
                                    var baseName = query.Context.Connection.GetMappedFieldName(includer.Table.Alias + "_");
                                    foreach (System.Data.DataColumn item in row.Table.Columns)
                                    {
                                        if (item.ColumnName.StartsWith(baseName, StringComparison.CurrentCultureIgnoreCase))
                                        {
                                            fieldName = baseName + Extensions.GetColumnName(member);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        if (string.IsNullOrEmpty(fieldName))
                        {
                            fieldName = Extensions.GetColumnName(bindingMember.Key);
                            if (!row.Table.Columns.Contains(fieldName))
                                fieldName = Extensions.GetColumnName(member);
                        }
                    }
                }
                else
                {
                    if (bindingMember.Value != null)
                    {
                        if (bindingMember.Value is MemberInitExpression)
                        {
                            this.SetData(query, entity, member, bindingMember.Value as MemberInitExpression, type, row);
                            counter++;
                            continue;
                        }
                        else if (member.GetMemberInfoType().IsDataEntity() || member.GetMemberInfoType().IsPOCOEntity())
                        {
                            var refEntity = Activator.CreateInstance(member.GetMemberInfoType());
                            var prop = bindingMember.Key as PropertyInfo;
                            prop.SetValue(entity, refEntity);

                            var table = query.Data.MainTable.Joins.Where(op => op.JoinOn == Extensions.GetForeignKeyName(member) && op.EntityType == refEntity.GetType()).FirstOrDefault();
                            var properties = member.GetMemberInfoType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(op => !op.PropertyType.IsDataEntity() && !op.PropertyType.IsQueryableDataSet());
                            foreach (var p in properties)
                            {
                                fieldName = Extensions.GetColumnName(p);
                                fieldName = query.Context.Connection.GetMappedFieldName(table.Alias + "_" + fieldName);
                                if (p.PropertyType.IsPrimitiveType() && row.Table.Columns.Contains(fieldName) && row[fieldName] != DBNull.Value)
                                {
                                    try
                                    {
                                        p.SetValue(refEntity, p.PropertyType.ConvertData(row[fieldName]));
                                    }
                                    catch (Exception)
                                    {
                                        Console.WriteLine($"{fieldName} property could not be set for {entity.GetType().FullName}");
                                    }
                                }
                            }
                            counter++;
                            continue;
                        }
                        else
                        {
                            fieldName = this.GetFieldName(query, bindingMember.Key, bindingMember.Value);
                            if (!row.Table.Columns.Contains(fieldName))
                                fieldName = this.GetFieldName(query, member, bindingMember.Value);
                        }
                    }
                    else
                    {
                        fieldName = query.Context.Connection.GetMappedFieldName(Extensions.GetColumnName(bindingMember.Key));
                        if (!row.Table.Columns.Contains(fieldName))
                            fieldName = query.Context.Connection.GetMappedFieldName(Extensions.GetColumnName(member));
                    }
                }
                this.SetData(member, fieldName, row, type, entity, bindingMember);
                counter++;
            }
        }
        private void SetData(MemberInfo member, string fieldName, System.Data.DataRow row, Type type, object entity, KeyValuePair<MemberInfo, Expression> bindingMember)
        {
            var memberType = member.GetMemberInfoType();
            if (bindingMember.Key == null)
                return;

            var p = type.GetProperty(member.Name);
            if(p == null)
                p = type.GetProperty(bindingMember.Key.Name);

            if (bindingMember.Value != null)
            {
                if (bindingMember.Value is ConstantExpression)
                {
                    p.SetValue(entity, (bindingMember.Value as ConstantExpression).Value);
                }
                else if (bindingMember.Value is MemberExpression)
                {
                    var propInfo = (bindingMember.Value as MemberExpression).Member as PropertyInfo;
                    if (propInfo != null && propInfo.IsStaticProperty())
                    {
                        p.SetValue(entity, propInfo.GetStaticPropertyValue());
                    }
                }
            }
            if (!string.IsNullOrEmpty(fieldName) && memberType.IsPrimitiveType() && row.Table.Columns.Contains(fieldName) && row[fieldName] != DBNull.Value)
            {
                try
                {
                    if (bindingMember.Value != null)
                    {
                        p.SetValue(entity, p.PropertyType.ConvertData(row[fieldName]));
                    }
                    else
                    {
                        p.SetValue(entity, p.PropertyType.ConvertData(row[fieldName]));
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine($"{fieldName} property could not be set for {entity.GetType().FullName}");
                }
            }
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
