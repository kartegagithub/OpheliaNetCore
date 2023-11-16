using AngleSharp.Dom;
using Ophelia.Data.Model;
using Ophelia.Data.Querying.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Ophelia.Data
{
    public class DataContext : IDisposable
    {
        private Connection _Connection;
        [ThreadStatic]
        protected static Dictionary<Type, DataContext> _Currents;

        public int ExecutionTimeout { get; set; }
        public Dictionary<string, string> NamespaceMap { get; set; }
        public Dictionary<string, string> TableMap { get; set; }
        public Dictionary<string, string> FieldMap { get; set; }
        public List<Type> ContextEntities { get; set; }
        private Dictionary<Type, Repository> RepositoryCache { get; set; } = new Dictionary<Type, Repository>();
        public static DataContext Current
        {
            get
            {
                var type = MethodBase.GetCurrentMethod().ReflectedType;
                if (!GetCurrents().ContainsKey(type))
                {
                    _Currents[type] = (DataContext)CreateInstance(MethodBase.GetCurrentMethod().ReflectedType);
                }
                return _Currents[type];
            }
        }

        private static Dictionary<Type, DataContext> GetCurrents()
        {
            if (_Currents == null)
                _Currents = new Dictionary<Type, DataContext>();
            return _Currents;
        }

        public Connection Connection
        {
            get
            {
                return this._Connection;
            }
        }
        public DataConfiguration Configuration { get; set; }

        public Repository<T> GetRepository<T>() where T : class
        {
            if (this.RepositoryCache.ContainsKey(typeof(T)))
                return (Repository<T>)this.RepositoryCache[typeof(T)];

            var repository = new Repository<T>(this);
            this.RepositoryCache.Add(typeof(T), repository);
            return repository;
        }
        public bool SaveChanges<T>(T entity) where T : class
        {
            var repository = this.GetRepository<T>();
            return repository.SaveChanges(entity);
        }
        public virtual void OnBeforeSaveChanges(object entity)
        {

        }
        public virtual T Create<T>() where T : class
        {
            var repository = this.GetRepository<T>();
            return repository.Create();
        }
        public virtual Model.QueryableDataSet<T> GetQuery<T>() where T : class
        {
            var repository = this.GetRepository<T>();
            return repository.GetQuery();
        }
        public virtual T Track<T>(T entity) where T : class
        {
            var repository = this.GetRepository<T>();
            var trackedEntity = repository.Track(entity);
            return trackedEntity;
        }
        internal SelectQuery CreateSelectQuery(Type entityType)
        {
            return new SelectQuery(this, entityType);
        }

        internal SelectQuery CreateSelectQuery(MethodCallExpression expression, Model.QueryableDataSet data)
        {
            var tmpType = this.GetInnerType(expression, data);
            if (tmpType != data.ElementType)
                data.InnerType = tmpType;
            return new SelectQuery(this, data, expression);
        }
        private Type GetInnerType(MethodCallExpression expression, Model.QueryableDataSet data)
        {
            if (expression == null)
                return null;

            var type = expression.Arguments[0].Type.GenericTypeArguments.FirstOrDefault();
            if (type != null)
            {
                var tmpType = GetInnerType(expression.Arguments[0] as MethodCallExpression, data);
                if (tmpType == null)
                    return type;
                else
                    return tmpType;
            }
            else
                return type;
        }
        public virtual QueryLogger CreateLogger()
        {
            return new QueryLogger();
        }
        internal DeleteQuery CreateDeleteQuery(object data)
        {
            return new DeleteQuery(this, data);
        }
        internal DeleteQuery CreateDeleteQuery(MethodCallExpression expression, Model.QueryableDataSet data)
        {
            return new DeleteQuery(this, data, expression);
        }

        internal InsertQuery CreateInsertQuery(MethodCallExpression expression, Model.QueryableDataSet data)
        {
            return new InsertQuery(this, data, expression);
        }

        internal InsertQuery CreateInsertQuery(object data)
        {
            return new InsertQuery(this, data);
        }

        internal UpdateQuery CreateUpdateQuery(object data)
        {
            return new UpdateQuery(this, data);
        }
        internal UpdateQuery CreateUpdateQuery(MethodCallExpression expression, Model.QueryableDataSet data, Expressions.UpdateExpression[] updaters)
        {
            return new UpdateQuery(this, data, expression, updaters);
        }
        internal UpdateQuery CreateUpdateQuery(MethodCallExpression expression, Model.QueryableDataSet data, Expressions.UpdateExpression updater)
        {
            return new UpdateQuery(this, data, expression, updater);
        }

        internal UpdateQuery CreateUpdateQuery(MethodCallExpression expression, Model.QueryableDataSet data)
        {
            return new UpdateQuery(this, data, expression);
        }

        public DataContext(DatabaseType Type, string ConnectionString) : this()
        {
            this._Connection = new Connection(this, Type, ConnectionString);
        }

        public DataContext()
        {
            this.Configuration = new DataConfiguration();
            this.NamespaceMap = new Dictionary<string, string>();
            this.TableMap = new Dictionary<string, string>();
            this.FieldMap = new Dictionary<string, string>();
            this.ContextEntities = new List<Type>();
            this.Configure();
            this._Connection = new Connection(this, this.GetDatabaseType(), this.GetConnectionString());
        }
        protected virtual void Configure()
        {

        }
        protected virtual string GetConnectionString()
        {
            return "";
        }
        protected virtual DatabaseType GetDatabaseType()
        {
            return DatabaseType.SQLServer;
        }
        internal DataContext GetContext(Type type)
        {
            DataContext defaultContext = this;
            foreach (var context in GetCurrents())
            {
                if (context.Value.ContextEntities.Count > 0 && context.Value.ContextEntities.Contains(type))
                    return context.Value;
                else if (context.Value.ContextEntities.Count == 0)
                    defaultContext = context.Value;
            }
            return defaultContext;
        }
        internal bool ContainsEntityType(Type type)
        {
            if (GetCurrents().Count == 1)
                return true;

            if (this.ContextEntities.Count > 0 && this.ContextEntities.Contains(type))
                return true;

            foreach (var context in GetCurrents())
            {
                if (context.Value != this)
                {
                    if (context.Value.ContextEntities.Count > 0 && context.Value.ContextEntities.Contains(type))
                        return false;
                }
            }
            return this.ContextEntities.Count == 0;
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.TableMap = null;
            this.NamespaceMap = null;
            this.ContextEntities = null;
            this.RepositoryCache.Clear();
            this.RepositoryCache = null;
            this.Connection.Close();
            this.Connection.Dispose();
        }
        public virtual void OnAfterEntityLoaded(object entity)
        {

        }
        protected virtual DataContext MapTable<T>(string toType)
        {
            return this.MapTable(typeof(T).Name, toType);
        }
        protected virtual DataContext MapTable(string fromType, string toType)
        {
            if (!string.IsNullOrEmpty(fromType) && !string.IsNullOrEmpty(toType) && !this.TableMap.ContainsKey(fromType))
                this.TableMap.Add(fromType, toType);
            return this;
        }
        protected virtual DataContext MapNamespace(string fromSpace, string toSpace)
        {
            if (!string.IsNullOrEmpty(fromSpace) && !string.IsNullOrEmpty(toSpace) && !this.NamespaceMap.ContainsKey(fromSpace))
                this.NamespaceMap.Add(fromSpace, toSpace);
            return this;
        }
        protected virtual DataContext MapField(string fromField, string toField)
        {
            if (!string.IsNullOrEmpty(fromField) && !string.IsNullOrEmpty(toField) && !this.FieldMap.ContainsKey(toField))
                this.FieldMap.Add(fromField, toField);
            return this;
        }
        public static object CreateInstance(Type Type, params object[] parameters)
        {
            foreach (Assembly a in ReflectionExtensions.GetValidAssemblies())
            {
                try
                {
                    var Types = a.GetTypes();
                    if (Types != null)
                    {
                        Types = Types.Where(op => op.IsSubclassOf(Type)).ToArray();
                        if (Types != null && Types.Length > 0)
                        {
                            foreach (var tempType in Types)
                            {
                                if (tempType.FullName != Type.FullName)
                                {
                                    if (parameters != null)
                                        return Activator.CreateInstance(tempType, parameters);
                                    else
                                        return Activator.CreateInstance(tempType);
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    ReflectionExtensions.ExcludedAssemblies.Add(a.FullName);
                    continue;
                }
            }
            if (parameters != null)
                return Activator.CreateInstance(Type, parameters);
            else
                return Activator.CreateInstance(Type);
        }
        public QueryBuilder CreateSQLBuilder()
        {
            return QueryBuilder.Init(this);
        }
        public static void Close()
        {
            if (_Currents != null)
            {
                foreach (var item in _Currents)
                {
                    item.Value.Dispose();
                }
                _Currents.Clear();
            }
            _Currents = null;
        }
    }
}
