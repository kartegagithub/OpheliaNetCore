using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Ophelia
{
    public static class IQueryableExtensions
    {
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
                throw new ArgumentException("Collection too large - execution will cause stack overflow", nameof(collection));

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
    }
}
