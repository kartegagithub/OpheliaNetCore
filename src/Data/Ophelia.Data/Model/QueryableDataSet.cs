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
        private Expression? _Expression;
        private IQueryProvider _Provider;
        protected IList? _list;
        internal QueryData ExtendedData { get; set; }
        internal bool HasChanged { get; set; }
        public bool TrackChanges { get; set; }
        internal bool DistinctEnabled { get; set; }

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
            var colLoadStartTime = Utility.Now;

            try
            {
                this.Count = data.Rows.Count;
                loadLog.Count = data.Rows.Count;

                var thisType = this.GetType();
                Type type = null;
                if ((query.Data.Selectors == null || query.Data.Selectors.Count == 0) && (query.Data.Groupers == null || query.Data.Groupers.Count == 0))
                    type = this.InnerType != null ? this.InnerType : this.ElementType;
                else
                    type = this.ElementType;

                if (type.GenericTypeArguments.Any() && !type.IsAnonymousType())
                    type = type.GenericTypeArguments.FirstOrDefault();

                var selectedFields = this.GetSelectedFields(query, type.IsPrimitiveType() ? this.InnerType : type);

                if (!thisType.IsGenericType || !thisType.GenericTypeArguments.FirstOrDefault().Name.Contains("OGrouping"))
                {
                    foreach (DataRow row in data.Rows)
                    {
                        var entLoadLoad = Utility.Now;

                        if (type.IsPrimitiveType())
                            this._list.Add(type.ConvertData(row[0]));
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
                            object entity = Activator.CreateInstance(type);
                            var isDataEntity = entity.GetType().IsDataEntity();
                            if (isDataEntity)
                                (entity as DataEntity).InternalTracker.State = EntityState.Loading;

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
                            if (isDataEntity)
                                (entity as DataEntity).InternalTracker.State = EntityState.Loaded;

                            if (this.TrackChanges && !isDataEntity)
                            {
                                var proxyEntity = Proxy.InternalProxyGenerator.Create(type, entity);
                                this._list.Add(proxyEntity);
                                query.Context.OnAfterEntityLoaded(proxyEntity);
                            }
                            else
                            {
                                this._list.Add(entity);
                                query.Context.OnAfterEntityLoaded(entity);
                            }
                        }

                        var duration = Utility.Now.Subtract(entLoadLoad).TotalMilliseconds;
                        if (loadLog.EntityLoadDuration < duration)
                            loadLog.EntityLoadDuration = duration;
                    }
                }
                else
                {
                    var types = new List<Type>();
                    var queryableType = typeof(QueryableDataSet<>).MakeGenericType(this.InnerType);
                    var groupingType = typeof(OGrouping<,>).MakeGenericType(type, this.InnerType);

                    var clonedData = query.Data.Serialize();
                    clonedData.Groupers.Clear();
                    clonedData.Sorters.RemoveAll(op => op.Name == "Key");

                    var counter = -1;
                    foreach (DataRow row in data.Rows)
                    {
                        var queryable = (QueryableDataSet)Activator.CreateInstance(queryableType, query.Context);
                        queryable.ExtendData(clonedData);
                        counter++;
                        var count = Convert.ToInt64(row[query.Context.Connection.GetMappedFieldName("Count")]);

                        object aEntity = null;
                        if (type.IsPrimitiveType())
                        {
                            aEntity = type.ConvertData(row[0]);
                        }
                        else if (type.IsAnonymousType())
                        {
                            var parameters = new List<object>();
                            foreach (var item in selectedFields)
                            {
                                var fieldName = query.Context.Connection.GetMappedFieldName(Extensions.GetColumnName(item.FieldProperty));
                                if (!data.Columns.Contains(fieldName))
                                    fieldName = query.Context.Connection.GetMappedFieldName(Extensions.GetColumnName(item.MappedProperty));

                                if (row.Table.Columns.Contains(fieldName))
                                {
                                    var value = item.MappedProperty.PropertyType.ConvertData(row[fieldName]);
                                    if (value == DBNull.Value)
                                    {
                                        if (item.MappedProperty.PropertyType.Name.StartsWith("String"))
                                            value = "";
                                    }
                                    parameters.Add(value);
                                }
                            }
                            aEntity = TypeExtensions.CreateAnonymous(type, parameters.ToArray());
                        }
                        else
                            aEntity = Activator.CreateInstance(type);

                        foreach (var item in selectedFields)
                        {
                            var fieldName = query.Context.Connection.GetMappedFieldName(Extensions.GetColumnName(item.FieldProperty));
                            if (!data.Columns.Contains(fieldName))
                                fieldName = query.Context.Connection.GetMappedFieldName(Extensions.GetColumnName(item.MappedProperty));

                            var value = item.MappedProperty.PropertyType.ConvertData(row[fieldName]);
                            if (value == DBNull.Value)
                            {
                                if (item.MappedProperty.PropertyType.Name.StartsWith("String"))
                                    value = "";
                            }

                            queryable = queryable.Where(item.FieldProperty, value);
                            if (!type.IsPrimitiveType() && !type.IsAnonymousType())
                                aEntity.SetPropertyValue(item.MappedProperty.Name, value);
                        }

                        queryable.ExtendData(clonedData);
                        if (query.Data.GroupPageSize == 0)
                            query.Data.GroupPageSize = 25;

                        var page = 1;
                        if (clonedData.GroupPagination.ContainsKey(counter))
                            page = clonedData.GroupPagination[counter];
                        var ctor = groupingType.GetConstructors().FirstOrDefault();
                        var oGrouping = ctor.Invoke(new object[] { aEntity, queryable.Paginate(page, query.Data.GroupPageSize), count });
                        this._list.Add(oGrouping);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                loadLog.ListLoadDuration = Utility.Now.Subtract(colLoadStartTime).TotalMilliseconds;
                if (query.Context.Configuration.LogEntityLoads)
                    query.Context.Connection.Logger.LogLoad(loadLog);
            }
        }

        private List<Reflection.ObjectField> GetSelectedFields(BaseQuery query, Type type)
        {
            var selectedFields = new List<Reflection.ObjectField>();
            if (query.Data.Selectors != null && query.Data.Selectors.Any())
            {
                foreach (var selector in query.Data.Selectors)
                {
                    if (!string.IsNullOrEmpty(selector.Name))
                    {
                        if (!selectedFields.Any(op => op.FieldProperty.Name == selector.Name))
                        {
                            selectedFields.Add(new Reflection.ObjectField()
                            {
                                FieldProperty = type.GetProperty(selector.Name),
                                MappedProperty = type.GetProperty(selector.Name)
                            });
                        }
                    }
                    else if (selector.SubSelector != null && !string.IsNullOrEmpty(selector.SubSelector.Name))
                    {
                        if (!selectedFields.Any(op => op.FieldProperty.Name == selector.SubSelector.Name))
                        {
                            selectedFields.Add(new Reflection.ObjectField()
                            {
                                FieldProperty = type.GetProperty(selector.SubSelector.Name),
                                MappedProperty = type.GetProperty(selector.SubSelector.Name)
                            });
                        }
                    }
                    else if (selector.SubSelector != null && selector.SubSelector.BindingMembers != null && selector.SubSelector.BindingMembers.Any())
                    {
                        var memberCounter = 0;
                        foreach (var item in selector.SubSelector.BindingMembers)
                        {
                            var field = new Reflection.ObjectField()
                            {
                                FieldProperty = selector.Members[memberCounter] as PropertyInfo,
                                MappedProperty = item.Key as PropertyInfo
                            };
                            if (!selectedFields.Any(op => op.FieldProperty.Name == field.FieldProperty.Name))
                                selectedFields.Add(field);

                            memberCounter++;
                        }
                    }
                    else if (selector.BindingMembers != null && selector.BindingMembers.Any())
                    {
                        var memberCounter = 0;
                        foreach (var item in selector.BindingMembers)
                        {
                            var field = new Reflection.ObjectField()
                            {
                                FieldProperty = selector.Members[memberCounter] as PropertyInfo,
                                MappedProperty = item.Key as PropertyInfo
                            };
                            if (!selectedFields.Any(op => op.FieldProperty.Name == field.FieldProperty.Name))
                                selectedFields.Add(field);

                            memberCounter++;
                        }
                    }
                }
            }
            else if (query.Data.Groupers != null && query.Data.Groupers.Any())
            {
                foreach (var grouper in query.Data.Groupers)
                {
                    if (!string.IsNullOrEmpty(grouper.Name))
                    {
                        if (!selectedFields.Any(op => op.FieldProperty.Name == grouper.Name))
                        {
                            selectedFields.Add(new Reflection.ObjectField()
                            {
                                FieldProperty = type.GetProperty(grouper.Name),
                                MappedProperty = type.GetProperty(grouper.Name)
                            });
                        }
                    }
                    else if (grouper.SubGrouper != null && !string.IsNullOrEmpty(grouper.SubGrouper.Name))
                    {
                        if (!selectedFields.Any(op => op.FieldProperty.Name == grouper.SubGrouper.Name))
                        {
                            selectedFields.Add(new Reflection.ObjectField()
                            {
                                FieldProperty = type.GetProperty(grouper.SubGrouper.Name),
                                MappedProperty = type.GetProperty(grouper.SubGrouper.Name)
                            });
                        }
                    }
                    else if (grouper.SubGrouper != null && grouper.SubGrouper.BindingMembers != null && grouper.SubGrouper.BindingMembers.Any())
                    {
                        var memberCounter = 0;
                        foreach (var item in grouper.SubGrouper.BindingMembers)
                        {
                            var field = new Reflection.ObjectField()
                            {
                                FieldProperty = grouper.Members[memberCounter] as PropertyInfo,
                                MappedProperty = item.Key as PropertyInfo
                            };
                            if (!selectedFields.Any(op => op.FieldProperty.Name == field.FieldProperty.Name))
                                selectedFields.Add(field);

                            memberCounter++;
                        }
                    }
                    else if (grouper.BindingMembers != null && grouper.BindingMembers.Any())
                    {
                        var memberCounter = 0;
                        foreach (var item in grouper.BindingMembers)
                        {
                            var field = new Reflection.ObjectField()
                            {
                                FieldProperty = grouper.Members[memberCounter] as PropertyInfo,
                                MappedProperty = item.Key as PropertyInfo
                            };
                            if (!selectedFields.Any(op => op.FieldProperty.Name == field.FieldProperty.Name))
                                selectedFields.Add(field);

                            memberCounter++;
                        }
                    }
                }
            }
            return selectedFields;
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
            return this.GetList();
        }
        public IEnumerator GetEnumerator()
        {
            this.EnsureLoad();
            return this._list.GetEnumerator();
        }
    }
}
