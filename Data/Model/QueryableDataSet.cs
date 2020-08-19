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
            //this._Expression = baseQuery.Expression;
            this._Expression = Expression.Constant(this);
            var type = DatabaseType.SQLServer;
            var innerContext = baseQuery.Provider.GetPropertyValue("InternalContext");
            if (innerContext != null)
            {
                if (Convert.ToString(innerContext.GetPropertyValue("ProviderName")) == "System.Data.SqlClient")
                    type = DatabaseType.SQLServer;
                this._Context = new DataContext(type, Convert.ToString(innerContext.GetPropertyValue("OriginalConnectionString")));
                this._Context.DBStructureCache.LoadFromEDMX(innerContext.GetPropertyStringValue("ConnectionStringName"));
            }
            else
                this._Context = new DataContext(type, "");
            var tmp = baseQuery.ElementType.FullName.Split('.');
            this._Context.Configuration.NamespacesToIgnore.Add(string.Join(".", tmp.Take(tmp.Length - 2)));
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
                            var aEntity = Activator.CreateInstance(type, parameters.ToArray());
                            this._list.Add(aEntity);
                        }
                        else
                        {
                            var entity = Activator.CreateInstance(type);
                            if (entity.GetType().IsDataEntity())
                            {
                                (entity as Model.DataEntity).Tracker.State = EntityState.Loading;
                            }
                            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(op => !op.PropertyType.IsDataEntity() && !op.PropertyType.IsQueryableDataSet());
                            foreach (var p in properties)
                            {
                                var fieldName = query.Context.Connection.GetMappedFieldName(p.Name);
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
                            if (entity.GetType().IsDataEntity())
                            {
                                (entity as Model.DataEntity).Tracker.State = EntityState.Loaded;
                            }
                            this._list.Add(entity);
                        }

                        var duration = DateTime.Now.Subtract(entLoadLoad).TotalMilliseconds;
                        if (loadLog.EntityLoadDuration < duration)
                            loadLog.EntityLoadDuration = duration;
                    }
                }
                else
                {
                    var types = new List<Type>();
                    var queryableType = typeof(QueryableDataSet<>).MakeGenericType(this.ElementType.GenericTypeArguments.LastOrDefault());
                    var entityType = this.ElementType.GenericTypeArguments[1];
                    var groupingType = typeof(OGrouping<,>).MakeGenericType(this.ElementType.GenericTypeArguments[0], entityType);
                    var clonedData = query.Data.Serialize();
                    clonedData.Groupers.Clear();
                    foreach (DataRow row in data.Rows)
                    {
                        var queryable = (QueryableDataSet)Activator.CreateInstance(queryableType, query.Context);
                        queryable.ExtendData(clonedData);
                        foreach (var item in query.Data.Groupers)
                        {
                            var count = Convert.ToInt64(row[query.Context.Connection.GetMappedFieldName("Counted")]);
                            if (!string.IsNullOrEmpty(item.Name))
                            {
                                var p = entityType.GetProperty(item.Name);
                                var fieldName = query.Context.Connection.GetMappedFieldName(item.Name);
                                var value = p.PropertyType.ConvertData(row[fieldName]);
                                if (value == DBNull.Value)
                                {
                                    if (p.PropertyType.Name.StartsWith("String"))
                                        value = "";
                                }

                                queryable = queryable.Where(p, value);
                                var ctor = groupingType.GetConstructors().FirstOrDefault();
                                var oGrouping = ctor.Invoke(new object[] { value, queryable, count });
                                //var oGrouping = Activator.CreateInstance(groupingType, );
                                this._list.Add(oGrouping);
                            }
                            else if (item.Members != null && item.Members.Count > 0)
                            {
                                var parameters = new List<object>();
                                foreach (var member in item.Members)
                                {
                                    var fieldName = query.Context.Connection.GetMappedFieldName(member.Name);
                                    Type memberType = null;
                                    switch (member.MemberType)
                                    {
                                        case MemberTypes.Field:
                                            memberType = ((FieldInfo)member).FieldType;
                                            break;
                                        case MemberTypes.Property:
                                            memberType = ((PropertyInfo)member).PropertyType;
                                            break;
                                        case MemberTypes.Event:
                                            memberType = ((EventInfo)member).EventHandlerType;
                                            break;
                                    }
                                    object value = memberType.ConvertData(row[fieldName]);
                                    queryable = queryable.Where(member, value);
                                    parameters.Add(value);
                                }
                                var oGrouping = Activator.CreateInstance(groupingType, Activator.CreateInstance(this.ElementType.GenericTypeArguments[0], parameters.ToArray()), queryable, count);
                                this._list.Add(oGrouping);
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
        public void ExtendData(QueryData data)
        {
            this.ExtendedData = data;
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
