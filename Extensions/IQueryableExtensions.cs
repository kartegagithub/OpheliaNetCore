using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Ophelia
{
    public static class IQueryableExtensions
    {
        public static IQueryable GroupBy<TElement>(this IQueryable<TElement> elements, params Expression<Func<TElement, object>>[] groupSelectors)
            where TElement : class
        {
            var columns = "";
            foreach (var item in groupSelectors)
            {
                if (!string.IsNullOrEmpty(columns))
                    columns += ",";
                var name = item.ParsePath();
                if (item.Body is MethodCallExpression)
                {
                    name = (item.Body as MethodCallExpression).Arguments.FirstOrDefault().ParsePath() + "ID";
                }
                else
                {
                    if (item.Body.Type.IsClass && !item.Body.Type.FullName.Contains("System."))
                        name += "ID";
                }
                
                if (name.IndexOf(".") > -1)
                    name = name + " as " + name.Replace(".", "");
                columns += name;
            }
            return elements.GroupBy("new (" + columns + ")", "it").OrderBy("Key");
        }
        public static IQueryable<TEntity> WhereIn<TEntity, TValue>
        (
            this IQueryable<TEntity> query,
            Expression<Func<TEntity, TValue>> selector,
            IEnumerable<TValue> collection
        )
        {
            ParameterExpression p = selector.Parameters.Single();

            if (!collection.Any()) return query.Where(x => false);

            if (collection.Count() > 3000)
                throw new ArgumentException("Collection too large - execution will cause stack overflow", "collection");

            IEnumerable<Expression> equals = collection.Select(value =>
               (Expression)Expression.Equal(selector.Body,
                    Expression.Constant(value, typeof(TValue))));

            Expression body = equals.Aggregate((accumulate, equal) =>
                Expression.Or(accumulate, equal));

            return query.Where(Expression.Lambda<Func<TEntity, bool>>(body, p));
        }

        public static IQueryable<TResult> Paginate<TResult>(this IQueryable<TResult> source, int page, int pageSize)
        {
            if (pageSize > 0 && page > 0)
            {
                int skip = Math.Max(pageSize * (page - 1), 0);
                return (IQueryable<TResult>)source.Skip(skip).Take(pageSize);
            }
            return source;
        }
        public static IQueryable Paginate(this IQueryable source, int page, int pageSize)
        {
            if (pageSize > 0 && page > 0)
            {
                int skip = Math.Max(pageSize * (page - 1), 0);
                return source.Skip(skip).Take(pageSize);
            }
            return source;
        }
    }
}
