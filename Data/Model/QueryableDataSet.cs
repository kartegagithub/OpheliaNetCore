using DocumentFormat.OpenXml.Drawing;
using Microsoft.EntityFrameworkCore;
using Ophelia.Data.Querying.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Ophelia.Data.Model
{
    public abstract class QueryableDataSet : IDisposable, IListSource, IQueryable
    {
        internal Type InnerType { get; set; }

        private DataContext _Context;
        private int _Count = -1;
        private long _TotalCount = -1;
        protected Expression _Expression;
        private IQueryProvider _Provider;
        protected IList _list;
        internal QueryData ExtendedData { get; set; }
        internal bool HasChanged { get; set; }
        internal bool DistinctEnabled { get; set; }
        public IList GroupedData { get; set; }

        internal int Count
        {
            get { return this._Count; }
            set { this._Count = value; }
        }

        public long TotalCount
        {
            get { return this._TotalCount; }
            internal set { this._TotalCount = value; }
        }

        public DataContext Context
        {
            get
            {
                return this._Context;
            }
        }
        protected virtual IList CreateList()
        {
            return new List<object>();
        }
        public QueryableDataSet(DbContext dbContext, IQueryable baseQuery, DatabaseType type) : this(baseQuery)
        {
            this._Context.Connection.ConnectionString = dbContext.Database.GetDbConnection().ConnectionString;
            this._Context.Connection.Type = type;
        }
        public Type GetOpheliaType()
        {
            return this.GetType();
        }
        public QueryableDataSet(IQueryable baseQuery)
        {
            var tmp = baseQuery.ElementType.FullName.Split('.');
            this._Context.Configuration.NamespacesToIgnore.Add(string.Join(".", tmp.Take(tmp.Length - 2)));
            this._Expression = Expression.Constant(this);
            this._list = this.CreateList();
        }
        public QueryableDataSet(DataContext Context)
        {
            this._Context = Context;
            this._list = this.CreateList();
        }

        public QueryableDataSet(DataContext Context, Expression expression) : this(Context)
        {
            this._Expression = expression;
        }

        public virtual bool ContainsListCollection
        {
            get
            {
                return true;
            }
        }

        public virtual Type ElementType
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual Expression Expression
        {
            get
            {
                if (this._Expression == null)
                    this._Expression = Expression.Constant(this);
                return this._Expression;
            }
        }

        public virtual IQueryProvider Provider
        {
            get
            {
                if (this._Provider == null)
                    this._Provider = new Querying.QueryProvider(this.Context, this);
                return this._Provider;
            }
        }

        public virtual Querying.QueryProvider InternalProvider
        {
            get
            {
                return (Querying.QueryProvider)this.Provider;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this._list.Clear();
            this._list = null;
        }

        public virtual IList GetList()
        {
            return this._list;
        }

        internal void Load(Querying.Query.BaseQuery query, DataTable data)
        {
            var loadLog = new Model.EntityLoadLog(query.Data.EntityType.Name);
            var colLoadStartTime = DateTime.Now;

            try
            {
                this.Count = data.Rows.Count;
                loadLog.Count = data.Rows.Count;
                if (query.Data.Groupers.Count == 0)
                {
                    foreach (DataRow row in data.Rows)
                    {
                        var entLoadLoad = DateTime.Now;

                        Type type = null;
                        if (query.Data.Selectors == null || query.Data.Selectors.Count == 0)
                        {
                            type = this.InnerType != null ? this.InnerType : this.ElementType;
                        }
                        else
                        {
                            type = this.ElementType;
                        }
                        if (type.IsPrimitiveType())
                        {
                            this._list.Add(this.ElementType.ConvertData(row[0]));
                        }
                        else if (type.IsAnonymousType())
                        {
                            var parameters = new List<object>();
                            foreach (DataColumn item in data.Columns)
                            {
                                parameters.Add(row[item.ColumnName]);
                            }
                            var aEntity = TypeExtensions.CreateAnonymous(type, parameters.ToArray());
                            this._list.Add(aEntity);
                        }
                        else
                        {
                            var entity = Activator.CreateInstance(type);
                            if (entity.GetType().IsDataEntity())
                            {
                                (entity as Model.DataEntity).Tracker.State = EntityState.Loading;
                            }
                            if (query.Data.Selectors == null || !query.Data.Selectors.Any())
                            {
                                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(op => !op.PropertyType.IsDataEntity() && !op.PropertyType.IsQueryableDataSet());
                                foreach (var p in properties)
                                {
                                    var fieldName = query.Context.Connection.GetMappedFieldName(Extensions.GetColumnName(p));
                                    if (p.PropertyType.IsPrimitiveType() && data.Columns.Contains(fieldName) && row[fieldName] != DBNull.Value)
                                    {
                                        try
                                        {
                                            p.SetValue(entity, p.PropertyType.ConvertData(row[fieldName]));
                                        }
                                        catch (Exception)
                                        {
                                            Console.WriteLine($"{fieldName} property could not be set for {entity.GetType().FullName}");
                                        }
                                    }
                                }
                                foreach (var includer in query.Data.Includers)
                                {
                                    includer.SetReferencedEntities(query, row, entity);
                                }
                            }
                            else
                            {
                                foreach (var selector in query.Data.Selectors)
                                {
                                    selector.SetData(query, entity, type, row);
                                }
                            }
                            if (entity.GetType().IsDataEntity())
                            {
                                (entity as Model.DataEntity).Tracker.State = EntityState.Loaded;
                            }
                            this._list.Add(entity);
                            query.Context.OnAfterEntityLoaded(entity);
                        }

                        var duration = DateTime.Now.Subtract(entLoadLoad).TotalMilliseconds;
                        if (loadLog.EntityLoadDuration < duration)
                            loadLog.EntityLoadDuration = duration;
                    }
                }
                else
                {
                    var dynamicObjectFields = (from grouper in query.Data.Groupers where !string.IsNullOrEmpty(grouper.Name) && !string.IsNullOrEmpty(grouper.TypeName) select new Ophelia.Reflection.ObjectField() { FieldName = grouper.Name, FieldType = Type.GetType(grouper.TypeName) }).ToList();
                    var dynamicObject = Ophelia.Reflection.ObjectBuilder.CreateNewObject(dynamicObjectFields);
                    var useDynamic = !this.ElementType.GenericTypeArguments.Any();
                    var types = new List<Type>();
                    var entityType = !useDynamic ? this.ElementType.GenericTypeArguments.LastOrDefault() : query.Data.EntityType;
                    var queryableType = typeof(QueryableDataSet<>).MakeGenericType(entityType);
                    var groupingType = typeof(OGrouping<,>).MakeGenericType(!useDynamic ? this.ElementType.GenericTypeArguments[0] : dynamicObject.GetType(), entityType);
                    var clonedData = query.Data.Serialize();
                    clonedData.Groupers.Clear();
                    clonedData.Sorters.RemoveAll(op => op.Name == "Key");
                    if (useDynamic)
                        this.GroupedData = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(groupingType));

                    var counter = -1;
                    foreach (DataRow row in data.Rows)
                    {
                        var queryable = (QueryableDataSet)Activator.CreateInstance(queryableType, query.Context);
                        queryable.ExtendData(clonedData);
                        counter++;
                        if (useDynamic)
                        {
                            dynamicObject = Activator.CreateInstance(dynamicObject.GetType());
                            var count = Convert.ToInt64(row[query.Context.Connection.GetMappedFieldName("Counted")]);
                            foreach (var item in dynamicObjectFields)
                            {
                                var p = entityType.GetProperty(item.FieldName);
                                var fieldName = query.Context.Connection.GetMappedFieldName(p.Name);
                                var value = p.PropertyType.ConvertData(row[fieldName]);
                                if (value == DBNull.Value)
                                {
                                    if (p.PropertyType.Name.StartsWith("String"))
                                        value = "";
                                }

                                queryable = queryable.Where(p, value);
                                dynamicObject.SetPropertyValue(item.FieldName, value);
                            }
                            queryable.ExtendData(clonedData);
                            if (query.Data.GroupPageSize == 0)
                                query.Data.GroupPageSize = 25;

                            var page = 1;
                            if (clonedData.GroupPagination.ContainsKey(counter))
                                page = clonedData.GroupPagination[counter];
                            var ctor = groupingType.GetConstructors().FirstOrDefault();
                            var oGrouping = ctor.Invoke(new object[] { dynamicObject, queryable.Paginate(page, query.Data.GroupPageSize).ToList(), count });
                            //var oGrouping = Activator.CreateInstance(groupingType, );
                            this.GroupedData.Add(oGrouping);
                        }
                        else
                        {
                            foreach (var item in query.Data.Groupers)
                            {
                                var count = Convert.ToInt64(row[query.Context.Connection.GetMappedFieldName("Counted")]);
                                var name = item.Name;
                                if (string.IsNullOrEmpty(name) && item.SubGrouper != null && !string.IsNullOrEmpty(item.SubGrouper.Name))
                                    name = item.SubGrouper.Name;
                                if (!string.IsNullOrEmpty(name))
                                {
                                    var p = entityType.GetProperty(name);
                                    var fieldName = query.Context.Connection.GetMappedFieldName(name);
                                    var value = p.PropertyType.ConvertData(row[fieldName]);
                                    if (value == DBNull.Value)
                                    {
                                        if (p.PropertyType.Name.StartsWith("String"))
                                            value = "";
                                    }

                                    queryable = queryable.Where(p, value);
                                    queryable.ExtendData(clonedData);
                                    var ctor = groupingType.GetConstructors().FirstOrDefault();
                                    var oGrouping = ctor.Invoke(new object[] { value, queryable, count });
                                    //var oGrouping = Activator.CreateInstance(groupingType, );
                                    this._list.Add(oGrouping);
                                }
                                else
                                {
                                    var members = item.BindingMembers;
                                    if (members == null && item.SubGrouper != null)
                                        members = item.SubGrouper.BindingMembers;
                                    if (members != null && members.Count > 0)
                                    {
                                        var parameters = new List<object>();
                                        foreach (var member in members)
                                        {
                                            var fieldName = query.Context.Connection.GetMappedFieldName(member.Key.Name);
                                            Type memberType = member.Key.GetMemberInfoType();
                                            object value = memberType.ConvertData(row[fieldName]);
                                            queryable = queryable.Where(Expression.Equal(member.Value, Expression.Constant(value)));
                                            parameters.Add(value);
                                        }
                                        queryable.ExtendData(clonedData);
                                        var oGrouping = Activator.CreateInstance(groupingType, Activator.CreateInstance(this.ElementType.GenericTypeArguments[0], parameters.ToArray()), queryable, count);
                                        this._list.Add(oGrouping);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                loadLog.ListLoadDuration = DateTime.Now.Subtract(colLoadStartTime).TotalMilliseconds;
                if (query.Context.Configuration.LogEntityLoads)
                    query.Context.Connection.Logger.LogLoad(loadLog);
            }
        }
        public void Add(object entity)
        {
            this.HasChanged = true;
            this._list.Add(entity);
            this.Count += 1;
            this.TotalCount += 1;
        }

        public bool Remove(long ID)
        {
            DataEntity foundEntity = null;
            foreach (DataEntity entity in this._list)
            {
                if (entity.ID == ID)
                {
                    foundEntity = entity;
                }
            }
            return this.Remove(foundEntity);
        }

        public bool Remove(object entity)
        {
            if (this._list.Contains(entity))
            {
                this.HasChanged = true;

                this._list.Remove(entity);
                this.Count -= 1;
                this.TotalCount -= 1;
                return true;
            }
            return false;
        }

        internal void AddItem(object entity)
        {
            this._list.Add(entity);
            this.Count += 1;
        }

        public void Clear()
        {
            this.HasChanged = true;
            this._list.Clear();
            this.Count = 0;
            this.TotalCount = 0;
        }
        public void ApplyExpression(Expression exp)
        {
            this._Expression = exp;
        }

        internal object GetItem(int index)
        {
            return this._list[index];
        }
        public QueryableDataSet ExtendData(QueryData data)
        {
            this.ExtendedData = data;
            return this;
        }
        public virtual void EnsureLoad()
        {
            if (this._Count == -1)
                (this.Provider as Querying.QueryProvider).LoadData(this);
        }
        public virtual IList ToList()
        {
            this.EnsureLoad();
            if (this.GroupedData != null)
                return this.GroupedData;
            return this.GetList();
        }
        public IEnumerator GetEnumerator()
        {
            this.EnsureLoad();
            return this._list.GetEnumerator();
        }
    }
}
