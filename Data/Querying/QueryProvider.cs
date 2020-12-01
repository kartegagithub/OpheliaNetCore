using System;
using System.Linq;
using System.Linq.Expressions;

namespace Ophelia.Data.Querying
{
    public class QueryProvider : IQueryProvider, IDisposable
    {
        private DataContext _Context;
        private Model.QueryableDataSet query;
        private int Take { get; set; }
        private int Skip { get; set; }
        public QueryProvider(DataContext Context, Model.QueryableDataSet query)
        {
            this._Context = Context;
            this.query = query;
        }
        public IQueryable CreateQuery(Expression expression, Type entityType)
        {
            var listType = typeof(Model.QueryableDataSet<>);
            var constructedListType = listType.MakeGenericType(entityType);

            return (IQueryable)Activator.CreateInstance(constructedListType, this._Context, expression);
        }
        public IQueryable CreateQuery(Expression expression)
        {
            var listType = typeof(Model.QueryableDataSet<>);
            var underlyingType = this.GetUnderlyingType(expression);
            var constructedListType = listType.MakeGenericType(underlyingType);

            return (IQueryable)Activator.CreateInstance(constructedListType, this._Context, expression);
        }
        private Type GetUnderlyingType(Expression expression)
        {
            if (expression.Type.GenericTypeArguments.Any())
                return expression.Type.GenericTypeArguments[0];
            else if (expression is MethodCallExpression)
                return GetUnderlyingType((expression as MethodCallExpression).Arguments[0]);
            else
                return null;
        }
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            var listType = typeof(Model.QueryableDataSet<>);
            var constructedListType = listType.MakeGenericType(typeof(TElement));
            return (IQueryable<TElement>)Activator.CreateInstance(constructedListType, this._Context, expression);
        }
        public IQueryable<TElement> CreateQuery<TElement, TBaseType>(Expression expression)
        {
            return new Model.QueryableDataSet<TElement>(this._Context, expression) { InnerType = typeof(TBaseType) };
        }

        public object Execute(Expression expression)
        {
            return this.Execute<object>(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            if (expression is MethodCallExpression)
                return this.GetResult<TResult>(this.query, (MethodCallExpression)expression);
            else if (expression is ConstantExpression)
                return this.GetResult<TResult>(this.query, (ConstantExpression)expression);

            return (TResult)Convert.ChangeType(null, typeof(TResult));
        }
        internal TResult GetResult<TResult>(Model.QueryableDataSet data, ConstantExpression expression)
        {
            return (TResult)Convert.ChangeType(null, typeof(TResult));
        }
        internal void LoadData(Model.QueryableDataSet data)
        {
            using (var query = this._Context.CreateSelectQuery(data.Expression as MethodCallExpression, data))
            {
                data.Load(query, query.GetData());
            }
        }
        internal TResult GetResult<TResult>(Model.QueryableDataSet data, MethodCallExpression expression)
        {
            switch (expression.Method.Name)
            {
                case "Count":
                case "Any":
                    if (data.TotalCount < 0)
                    {
                        using (var query = this._Context.CreateSelectQuery(expression, data))
                        {
                            data.TotalCount = Convert.ToInt64(query.Execute<TResult>(CommandType.Count));
                        }
                    }
                    if (expression.Method.Name.Equals("Any"))
                        return (TResult)Convert.ChangeType((data.TotalCount > 0), typeof(TResult));
                    return (TResult)Convert.ChangeType(data.TotalCount, typeof(TResult));
                case "Sum":
                    using (var query = this._Context.CreateSelectQuery(expression, data))
                    {
                        return query.Execute<TResult>();
                    }
                case "First":
                case "FirstOrDefault":
                case "Last":
                case "LastOrDefault":
                    if (data.Count == -1)
                        this.LoadData(data);

                    if (data.Count > 0)
                    {
                        if (expression.Method.Name == "First" || expression.Method.Name == "FirstOrDefault")
                            return (TResult)Convert.ChangeType(data.GetItem(0), typeof(TResult));
                        else
                            return (TResult)Convert.ChangeType(data.GetItem(data.Count - 1), typeof(TResult));
                    }
                    return default(TResult);
            }
            return (TResult)Convert.ChangeType(null, typeof(TResult));
        }
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {

        }
    }
}
