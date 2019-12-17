using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Reflection;
using Ophelia.Data.Model;

namespace Ophelia.Data
{
    public static class QueryableDataSetExtensions
    {
        /*         https://raw.githubusercontent.com/dotnet/corefx/master/src/System.Linq.Queryable/src/System/Linq/Queryable.cs             */
        public static QueryableDataSet<TSource> Include<TSource, TProperty>(this QueryableDataSet<TSource> source, Expression<Func<TSource, TProperty>> predicate)
        {
            return IncludeInternal(source, predicate, GetMethodInfoOf(() => QueryableDataSetExtensions.Include(default(QueryableDataSet<TSource>), default(Expression<Func<TSource, TProperty>>))), JoinType.Left);
        }
        public static QueryableDataSet<TSource> LeftInclude<TSource, TProperty>(this QueryableDataSet<TSource> source, Expression<Func<TSource, TProperty>> predicate)
        {
            return IncludeInternal(source, predicate, GetMethodInfoOf(() => QueryableDataSetExtensions.Include(default(QueryableDataSet<TSource>), default(Expression<Func<TSource, TProperty>>))), JoinType.Left);
        }
        public static QueryableDataSet<TSource> RightInclude<TSource, TProperty>(this QueryableDataSet<TSource> source, Expression<Func<TSource, TProperty>> predicate)
        {
            return IncludeInternal(source, predicate, GetMethodInfoOf(() => QueryableDataSetExtensions.Include(default(QueryableDataSet<TSource>), default(Expression<Func<TSource, TProperty>>))), JoinType.Right);
        }
        public static QueryableDataSet<TSource> RightOuterInclude<TSource, TProperty>(this QueryableDataSet<TSource> source, Expression<Func<TSource, TProperty>> predicate)
        {
            return IncludeInternal(source, predicate, GetMethodInfoOf(() => QueryableDataSetExtensions.Include(default(QueryableDataSet<TSource>), default(Expression<Func<TSource, TProperty>>))), JoinType.RightOuter);
        }
        public static QueryableDataSet<TSource> LeftOuterInclude<TSource, TProperty>(this QueryableDataSet<TSource> source, Expression<Func<TSource, TProperty>> predicate)
        {
            return IncludeInternal(source, predicate, GetMethodInfoOf(() => QueryableDataSetExtensions.Include(default(QueryableDataSet<TSource>), default(Expression<Func<TSource, TProperty>>))), JoinType.LeftOuter);
        }
        public static IQueryable<TSource> Include<TSource>(this IQueryable<TSource> source, string includePath)
        {
            return source;
        }
        public static QueryableDataSet<TSource> Include<TSource>(this QueryableDataSet<TSource> source, string includePath)
        {
            var includeMethod = GetMethodInfoOf(() => QueryableDataSetExtensions.Include(default(IQueryable<TSource>), default(string)));
            Expression includeCall = Expression.Call(null, includeMethod, new Expression[] { source.Expression, new Expressions.IncludeExpression(includePath, JoinType.Inner) });
            source.ApplyExpression(includeCall);
            return source;
        }

        private static QueryableDataSet<TSource> IncludeInternal<TSource, TProperty>(QueryableDataSet<TSource> source, Expression<Func<TSource, TProperty>> predicate, MethodInfo includeMethod, JoinType join)
        {
            Expression includeCall = Expression.Call(null, includeMethod, new Expression[] { source.Expression, new Expressions.IncludeExpression(predicate, join) });
            source.ApplyExpression(includeCall);
            return source;
        }
        internal static QueryableDataSet Where(this QueryableDataSet source, Expression predicate)
        {
            return (QueryableDataSet)source.InternalProvider.CreateQuery(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.Where(
                        default(QueryableDataSet),
                        default(Expression))),
                    new Expression[] { source.Expression, new Expressions.WhereExpression(predicate) }
                    ), source.ElementType);
        }
        internal static QueryableDataSet<TSource> Where<TSource>(this QueryableDataSet<TSource> source, Expression predicate)
        {
            return (QueryableDataSet<TSource>)source.InternalProvider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.Where(
                        default(QueryableDataSet<TSource>),
                        default(Expression<Func<TSource, bool>>))),
                    new Expression[] { source.Expression, new Expressions.WhereExpression(predicate) }
                    ));
        }
        public static QueryableDataSet<TSource> Where<TSource>(this QueryableDataSet<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            return (QueryableDataSet<TSource>)source.InternalProvider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.Where(
                        default(QueryableDataSet<TSource>),
                        default(Expression<Func<TSource, bool>>))),
                    new Expression[] { source.Expression, new Expressions.WhereExpression(predicate) }
                    ));
        }
        public static QueryableDataSet Where(this QueryableDataSet source, System.Reflection.MemberInfo[] tree, object value, Comparison comparison = Comparison.Equal)
        {
            ParameterExpression parameterExpression = Expression.Parameter(tree.FirstOrDefault().ReflectedType, "op");
            MemberExpression member = null;
            for (int i = 0; i < tree.Length; i++)
            {
                if (member != null)
                    member = Expression.MakeMemberAccess(member, tree[i]);
                else
                    member = Expression.MakeMemberAccess(parameterExpression, tree[i]);
            }
            var p = tree.LastOrDefault();
            Expression lambda = null;
            Expression subExpression = null;
            Expression constantExp = null;
            if (value == DBNull.Value)
                constantExp = Expression.Constant(value);
            else
                constantExp = Expression.Constant(value, p.GetMemberType());
            switch (comparison)
            {
                case Comparison.Equal:
                    subExpression = Expression.Equal(member, constantExp);
                    break;
                case Comparison.Different:
                    subExpression = Expression.NotEqual(member, constantExp);
                    break;
                case Comparison.Greater:
                    subExpression = Expression.GreaterThan(member, constantExp);
                    break;
                case Comparison.Less:
                    subExpression = Expression.LessThan(member, constantExp);
                    break;
                case Comparison.GreaterAndEqual:
                    subExpression = Expression.GreaterThanOrEqual(member, constantExp);
                    break;
                case Comparison.LessAndEqual:
                    subExpression = Expression.LessThanOrEqual(member, constantExp);
                    break;
                case Comparison.StartsWith:
                    subExpression = Expression.Call(member, typeof(string).GetMethod("StartsWith", new[] { typeof(string) }), constantExp);
                    break;
                case Comparison.EndsWith:
                    subExpression = Expression.Call(member, typeof(string).GetMethod("EndsWith", new[] { typeof(string) }), constantExp);
                    break;
                case Comparison.Contains:
                    subExpression = Expression.Call(member, typeof(string).GetMethod("Contains", new[] { typeof(string) }), constantExp);
                    break;
                case Comparison.In:
                    //TODO: Comparison.In eksik
                    break;
                case Comparison.Exists:
                    //TODO: Comparison.Exists eksik
                    break;
            }
            if (subExpression != null)
            {
                lambda = Expression.Lambda(subExpression, parameterExpression);
                return Where(source, lambda);
            }
            return source;
        }
        public static QueryableDataSet<TSource> Where<TSource>(this QueryableDataSet<TSource> source, System.Reflection.MemberInfo[] tree, object value, Comparison comparison = Comparison.Equal)
        {
            ParameterExpression parameterExpression = Expression.Parameter(tree.FirstOrDefault().ReflectedType, "op");
            MemberExpression member = null;
            for (int i = 0; i < tree.Length; i++)
            {
                if (member != null)
                    member = Expression.MakeMemberAccess(member, tree[i]);
                else
                    member = Expression.MakeMemberAccess(parameterExpression, tree[i]);
            }
            var p = tree.LastOrDefault();
            Expression<Func<TSource, bool>> lambda = null;
            Expression subExpression = null;
            Expression constantExp = null;
            if (value == DBNull.Value)
                constantExp = Expression.Constant(value);
            else
                constantExp = Expression.Constant(value, p.GetMemberType());
            switch (comparison)
            {
                case Comparison.Equal:
                    subExpression = Expression.Equal(member, constantExp);
                    break;
                case Comparison.Different:
                    subExpression = Expression.NotEqual(member, constantExp);
                    break;
                case Comparison.Greater:
                    subExpression = Expression.GreaterThan(member, constantExp);
                    break;
                case Comparison.Less:
                    subExpression = Expression.LessThan(member, constantExp);
                    break;
                case Comparison.GreaterAndEqual:
                    subExpression = Expression.GreaterThanOrEqual(member, constantExp);
                    break;
                case Comparison.LessAndEqual:
                    subExpression = Expression.LessThanOrEqual(member, constantExp);
                    break;
                case Comparison.StartsWith:
                    subExpression = Expression.Call(member, typeof(string).GetMethod("StartsWith", new[] { typeof(string) }), constantExp);
                    break;
                case Comparison.EndsWith:
                    subExpression = Expression.Call(member, typeof(string).GetMethod("EndsWith", new[] { typeof(string) }), constantExp);
                    break;
                case Comparison.Contains:
                    subExpression = Expression.Call(member, typeof(string).GetMethod("Contains", new[] { typeof(string) }), constantExp);
                    break;
                case Comparison.In:
                    //TODO: Comparison.In eksik
                    break;
                case Comparison.Exists:
                    //TODO: Comparison.Exists eksik
                    break;
            }
            if (subExpression != null)
            {
                lambda = Expression.Lambda<Func<TSource, bool>>(subExpression, parameterExpression);
                return Where(source, lambda);
            }
            return source;
        }
        public static QueryableDataSet Where(this QueryableDataSet source, System.Reflection.MemberInfo p, object value, Comparison comparison = Comparison.Equal)
        {
            return source.Where(new MemberInfo[] { p }, value, comparison);
        }
        public static QueryableDataSet Where(this QueryableDataSet source, System.Reflection.PropertyInfo p, object value, Comparison comparison = Comparison.Equal)
        {
            return source.Where(new PropertyInfo[] { p }, value, comparison);
        }
        public static QueryableDataSet<TSource> Where<TSource>(this QueryableDataSet<TSource> source, System.Reflection.PropertyInfo p, object value, Comparison comparison = Comparison.Equal)
        {
            return source.Where(new PropertyInfo[] { p }, value, comparison);
        }

        public static QueryableDataSet<TSource> Where<TSource>(this QueryableDataSet<TSource> source, Expressions.InExpression value)
        {
            return (QueryableDataSet<TSource>)source.InternalProvider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.Where(
                        default(QueryableDataSet<TSource>),
                        default(Expressions.InExpression))),
                    new Expression[] { source.Expression, value }
                    ));
        }

        public static QueryableDataSet<TSource> Where<TSource>(this QueryableDataSet<TSource> source, Expression<Func<TSource, int, bool>> predicate)
        {
            return (QueryableDataSet<TSource>)source.InternalProvider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.Where(
                        default(QueryableDataSet<TSource>),
                        default(Expression<Func<TSource, int, bool>>))),
                    new Expression[] { source.Expression, new Expressions.WhereExpression(predicate) }
                    ));
        }

        public static QueryableDataSet<TResult> Select<TSource, TResult>(this QueryableDataSet<TSource> source, Expression<Func<TSource, TResult>> selector)
        {
            return (QueryableDataSet<TResult>)source.InternalProvider.CreateQuery<TResult, TSource>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.Select(
                        default(QueryableDataSet<TSource>),
                        default(Expression<Func<TSource, TResult>>))),
                    new Expression[] { source.Expression, new Expressions.SelectExpression(selector) }
                    ));
        }

        public static QueryableDataSet<TResult> Select<TSource, TResult>(this QueryableDataSet<TSource> source, Expression<Func<TSource, int, TResult>> selector)

            where TResult : class
        {
            return (QueryableDataSet<TResult>)source.InternalProvider.CreateQuery<TResult, TSource>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.Select(
                        default(QueryableDataSet<TSource>),
                        default(Expression<Func<TSource, int, TResult>>))),
                    new Expression[] { source.Expression, new Expressions.SelectExpression(selector) }
                    ));
        }
        public static QueryableDataSet<TSource> CombineExpression<TSource>(this QueryableDataSet<TSource> source, Expression exp)
        {
            return (QueryableDataSet<TSource>)source.InternalProvider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.CombineExpression(
                        default(QueryableDataSet<TSource>),
                        default(Expression))),
                    new Expression[] { source.Expression, exp }
                    ));
        }
        //public static IQueryable<TResult> SelectMany<TSource, TResult>(this QueryableDataSet<TSource> source, Expression<Func<TSource, IEnumerable<TResult>>> selector)
        //{
        //    return source.InternalProvider.CreateQuery<TResult>(
        //        Expression.Call(
        //            null,
        //            GetMethodInfoOf(() => Queryable.SelectMany(
        //                default(QueryableDataSet<TSource>),
        //                default(Expression<Func<TSource, IEnumerable<TResult>>>))),
        //            new Expression[] { source.Expression, new Collections.Expressions.SelectExpression(selector) }
        //            ));
        //}

        //public static IQueryable<TResult> SelectMany<TSource, TResult>(this QueryableDataSet<TSource> source, Expression<Func<TSource, int, IEnumerable<TResult>>> selector)
        //{
        //    return source.InternalProvider.CreateQuery<TResult>(
        //        Expression.Call(
        //            null,
        //            GetMethodInfoOf(() => Queryable.SelectMany(
        //                default(QueryableDataSet<TSource>),
        //                default(Expression<Func<TSource, int, IEnumerable<TResult>>>))),
        //            new Expression[] { source.Expression, Expression.Quote(selector) }
        //            ));
        //}

        //public static IQueryable<TResult> SelectMany<TSource, TCollection, TResult>(this QueryableDataSet<TSource> source, Expression<Func<TSource, int, IEnumerable<TCollection>>> collectionSelector, Expression<Func<TSource, TCollection, TResult>> resultSelector)
        //{
        //    return source.InternalProvider.CreateQuery<TResult>(
        //        Expression.Call(
        //            null,
        //            GetMethodInfoOf(() => Queryable.SelectMany(
        //                default(QueryableDataSet<TSource>),
        //                default(Expression<Func<TSource, int, IEnumerable<TCollection>>>),
        //                default(Expression<Func<TSource, TCollection, TResult>>))),
        //            new Expression[] { source.Expression, Expression.Quote(collectionSelector), Expression.Quote(resultSelector) }
        //            ));
        //}

        //public static IQueryable<TResult> SelectMany<TSource, TCollection, TResult>(this QueryableDataSet<TSource> source, Expression<Func<TSource, IEnumerable<TCollection>>> collectionSelector, Expression<Func<TSource, TCollection, TResult>> resultSelector)
        //{
        //    return source.InternalProvider.CreateQuery<TResult>(
        //        Expression.Call(
        //            null,
        //            GetMethodInfoOf(() => Queryable.SelectMany(
        //                default(QueryableDataSet<TSource>),
        //                default(Expression<Func<TSource, IEnumerable<TCollection>>>),
        //                default(Expression<Func<TSource, TCollection, TResult>>))),
        //            new Expression[] { source.Expression, Expression.Quote(collectionSelector), Expression.Quote(resultSelector) }
        //            ));
        //}

        public static Expression GetSourceExpression(this QueryableDataSet source)
        {
            var q = source as QueryableDataSet;
            if (q != null) return q.Expression;
            return Expression.Constant(source, typeof(QueryableDataSet));
        }

        //public static IQueryable<TResult> Join<TOuter, TInner, TKey, TResult>(this QueryableDataSet<TOuter> outer, IEnumerable<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, TInner, TResult>> resultSelector)
        //{
        //    return outer.InternalProvider.CreateQuery<TResult>(
        //        Expression.Call(
        //            null,
        //            GetMethodInfoOf(() => Queryable.Join(
        //                default(QueryableDataSet<TOuter>),
        //                default(IEnumerable<TInner>),
        //                default(Expression<Func<TOuter, TKey>>),
        //                default(Expression<Func<TInner, TKey>>),
        //                default(Expression<Func<TOuter, TInner, TResult>>))),
        //            new Expression[] {
        //                outer.Expression,
        //                GetSourceExpression(inner),
        //                Expression.Quote(outerKeySelector),
        //                Expression.Quote(innerKeySelector),
        //                Expression.Quote(resultSelector)
        //                }
        //            ));
        //}

        //public static IQueryable<TResult> Join<TOuter, TInner, TKey, TResult>(this QueryableDataSet<TOuter> outer, IEnumerable<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, TInner, TResult>> resultSelector, IEqualityComparer<TKey> comparer)
        //{
        //    return outer.InternalProvider.CreateQuery<TResult>(
        //        Expression.Call(
        //            null,
        //            GetMethodInfoOf(() => Queryable.Join(
        //                default(QueryableDataSet<TOuter>),
        //                default(IEnumerable<TInner>),
        //                default(Expression<Func<TOuter, TKey>>),
        //                default(Expression<Func<TInner, TKey>>),
        //                default(Expression<Func<TOuter, TInner, TResult>>),
        //                default(IEqualityComparer<TKey>))),
        //            new Expression[] {
        //                outer.Expression,
        //                GetSourceExpression(inner),
        //                Expression.Quote(outerKeySelector),
        //                Expression.Quote(innerKeySelector),
        //                Expression.Quote(resultSelector),
        //                Expression.Constant(comparer, typeof(IEqualityComparer<TKey>))
        //                }
        //            ));
        //}

        //public static IQueryable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this QueryableDataSet<TOuter> outer, IEnumerable<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, IEnumerable<TInner>, TResult>> resultSelector)
        //{
        //    return outer.InternalProvider.CreateQuery<TResult>(
        //        Expression.Call(
        //            null,
        //            GetMethodInfoOf(() => Queryable.GroupJoin(
        //                default(QueryableDataSet<TOuter>),
        //                default(IEnumerable<TInner>),
        //                default(Expression<Func<TOuter, TKey>>),
        //                default(Expression<Func<TInner, TKey>>),
        //                default(Expression<Func<TOuter, IEnumerable<TInner>, TResult>>))),
        //            new Expression[] {
        //                outer.Expression,
        //                GetSourceExpression(inner),
        //                Expression.Quote(outerKeySelector),
        //                Expression.Quote(innerKeySelector),
        //                Expression.Quote(resultSelector) }
        //            ));
        //}

        //public static IQueryable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this QueryableDataSet<TOuter> outer, IEnumerable<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, IEnumerable<TInner>, TResult>> resultSelector, IEqualityComparer<TKey> comparer)
        //{
        //    return outer.InternalProvider.CreateQuery<TResult>(
        //        Expression.Call(
        //            null,
        //            GetMethodInfoOf(() => Queryable.GroupJoin(
        //                default(QueryableDataSet<TOuter>),
        //                default(IEnumerable<TInner>),
        //                default(Expression<Func<TOuter, TKey>>),
        //                default(Expression<Func<TInner, TKey>>),
        //                default(Expression<Func<TOuter, IEnumerable<TInner>, TResult>>),
        //                default(IEqualityComparer<TKey>))),
        //            new Expression[] {
        //                outer.Expression,
        //                GetSourceExpression(inner),
        //                Expression.Quote(outerKeySelector),
        //                Expression.Quote(innerKeySelector),
        //                Expression.Quote(resultSelector),
        //                Expression.Constant(comparer, typeof(IEqualityComparer<TKey>)) }
        //            ));
        //}
        public static QueryableDataSet<TSource> Exclude<TSource>(this QueryableDataSet<TSource> source, string keySelector)
        {
            return (QueryableDataSet<TSource>)source.InternalProvider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.Exclude(
                        default(QueryableDataSet<TSource>),
                        default(string))),
                    new Expression[] { source.Expression, new Expressions.ExcludeExpression(keySelector, false) }
                    ));
        }
        public static QueryableDataSet<TSource> ExcludeFromAll<TSource>(this QueryableDataSet<TSource> source, string keySelector)
        {
            return (QueryableDataSet<TSource>)source.InternalProvider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.ExcludeFromAll(
                        default(QueryableDataSet<TSource>),
                        default(string))),
                    new Expression[] { source.Expression, new Expressions.ExcludeExpression(keySelector, true) }
                    ));
        }
        public static QueryableDataSet<TSource> Exclude<TSource, TKey>(this QueryableDataSet<TSource> source, Expression<Func<TSource, TKey>> keySelector)
        {
            return (QueryableDataSet<TSource>)source.InternalProvider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.Exclude(
                        default(QueryableDataSet<TSource>),
                        default(Expression<Func<TSource, TKey>>))),
                    new Expression[] { source.Expression, new Expressions.ExcludeExpression(keySelector, false) }
                    ));
        }
        public static QueryableDataSet<TSource> OrderBy<TSource>(this QueryableDataSet<TSource> source, string propertyAndDirection)
        {
            var splitted = propertyAndDirection.Split(' ');
            var ascending = true;
            if (splitted.Length > 1)
                ascending = !(splitted[1].ToLower() == "desc" || splitted[1].ToLower() == "descending");
            return source.OrderBy(splitted[0], ascending);
        }
        public static QueryableDataSet<TSource> OrderBy<TSource>(this QueryableDataSet<TSource> source, string property, bool ascending = true)
        {
            return source.OrderBy(typeof(TSource).GetPropertyInfo(property), ascending);
        }
        public static QueryableDataSet<TSource> OrderBy<TSource>(this QueryableDataSet<TSource> source, PropertyInfo property, bool ascending = true)
        {
            return (QueryableDataSet<TSource>)source.InternalProvider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.OrderBy(
                        default(QueryableDataSet<TSource>),
                        default(object))),
                    new Expression[] { source.Expression, new Expressions.OrderExpression(Expression.Property(Expression.Parameter(typeof(TSource)), property), ascending, true) }
                    ));
        }
        public static QueryableDataSet<TSource> OrderBy<TSource>(this QueryableDataSet<TSource> source, object expression)
        {
            return source;
        }
        public static QueryableDataSet<TSource> OrderBy<TSource, TKey>(this QueryableDataSet<TSource> source, Expression<Func<TSource, TKey>> keySelector)
        {
            return (QueryableDataSet<TSource>)source.InternalProvider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.OrderBy(
                        default(QueryableDataSet<TSource>),
                        default(Expression<Func<TSource, TKey>>))),
                    new Expression[] { source.Expression, new Expressions.OrderExpression(keySelector, true, true) }
                    ));
        }

        public static QueryableDataSet<TSource> OrderByDescending<TSource, TKey>(this QueryableDataSet<TSource> source, Expression<Func<TSource, TKey>> keySelector)
        {
            return (QueryableDataSet<TSource>)source.InternalProvider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.OrderByDescending(
                        default(QueryableDataSet<TSource>),
                        default(Expression<Func<TSource, TKey>>))),
                    new Expression[] { source.Expression, new Expressions.OrderExpression(keySelector, false, true) }
                    ));
        }

        public static QueryableDataSet<TSource> ThenBy<TSource, TKey>(this QueryableDataSet<TSource> source, Expression<Func<TSource, TKey>> keySelector)
        {
            return (QueryableDataSet<TSource>)source.InternalProvider.CreateQuery<TSource>(
               Expression.Call(
                   null,
                   GetMethodInfoOf(() => QueryableDataSetExtensions.ThenBy(
                       default(QueryableDataSet<TSource>),
                       default(Expression<Func<TSource, TKey>>))),
                   new Expression[] { source.Expression, new Expressions.OrderExpression(keySelector, true) }
                   ));
        }

        public static QueryableDataSet<TSource> ThenByDescending<TSource, TKey>(this QueryableDataSet<TSource> source, Expression<Func<TSource, TKey>> keySelector)
        {
            return (QueryableDataSet<TSource>)source.InternalProvider.CreateQuery<TSource>(
               Expression.Call(
                   null,
                   GetMethodInfoOf(() => QueryableDataSetExtensions.ThenByDescending(
                       default(QueryableDataSet<TSource>),
                       default(Expression<Func<TSource, TKey>>))),
                   new Expression[] { source.Expression, new Expressions.OrderExpression(keySelector, false) }
                   ));
        }
        public static QueryableDataSet<TSource> Paginate<TSource>(this QueryableDataSet<TSource> source, int page, int pageSize)
        {
            if (pageSize > 0 && page > 0)
            {
                int skip = Math.Max(pageSize * (page - 1), 0);
                return source.Skip(skip).Take(pageSize);
            }
            return source;
        }

        public static QueryableDataSet<TSource> Take<TSource>(this QueryableDataSet<TSource> source, int count)
        {
            return (QueryableDataSet<TSource>)source.InternalProvider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.Take(
                        default(QueryableDataSet<TSource>),
                        default(int))),
                    new Expression[] { source.Expression, new Expressions.TakeExpression(count) }
                    ));
        }

        public static QueryableDataSet<TSource> Skip<TSource>(this QueryableDataSet<TSource> source, int count)
        {
            return (QueryableDataSet<TSource>)source.InternalProvider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.Skip(
                        default(QueryableDataSet<TSource>),
                        default(int))),
                    new Expression[] { source.Expression, new Expressions.SkipExpression(count) }
                    ));
        }

        public static QueryableDataSet<OGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this QueryableDataSet<TSource> source, Expression<Func<TSource, TKey>> keySelector)
        {
            return (QueryableDataSet<OGrouping<TKey, TSource>>)source.InternalProvider.CreateQuery<OGrouping<TKey, TSource>>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.GroupBy(
                        default(QueryableDataSet<TSource>),
                        default(Expression<Func<TSource, TKey>>))),
                    new Expression[] { source.Expression, new Expressions.GroupExpression(keySelector) }
                    ));
        }

        //public static QueryableDataSet<TSource> Union<TSource>(this QueryableDataSet<TSource> source1, IEnumerable<TSource> source2)
        //{
        //    return (QueryableDataSet<TSource>)source1.InternalProvider.CreateQuery<TSource>(
        //        Expression.Call(
        //            null,
        //            GetMethodInfoOf(() => Queryable.Union(
        //                default(QueryableDataSet<TSource>),
        //                default(QueryableDataSet<TSource>))),
        //            new Expression[] { source1.Expression, GetSourceExpression(source2) }
        //            ));
        //}

        //public static QueryableDataSet<TSource> Union<TSource>(this QueryableDataSet<TSource> source1, IEnumerable<TSource> source2, IEqualityComparer<TSource> comparer)
        //{
        //    return (QueryableDataSet<TSource>)source1.InternalProvider.CreateQuery<TSource>(
        //        Expression.Call(
        //            null,
        //            GetMethodInfoOf(() => Queryable.Union(
        //                default(QueryableDataSet<TSource>),
        //                default(QueryableDataSet<TSource>),
        //                default(IEqualityComparer<TSource>))),
        //            new Expression[] {
        //                source1.Expression,
        //                GetSourceExpression(source2),
        //                Expression.Constant(comparer, typeof(IEqualityComparer<TSource>))
        //                }
        //            ));
        //}

        //public static QueryableDataSet<TSource> Intersect<TSource>(this QueryableDataSet<TSource> source1, IEnumerable<TSource> source2)
        //{
        //    return (QueryableDataSet<TSource>)source1.InternalProvider.CreateQuery<TSource>(
        //        Expression.Call(
        //            null,
        //            GetMethodInfoOf(() => Queryable.Intersect(
        //                default(QueryableDataSet<TSource>),
        //                default(QueryableDataSet<TSource>))),
        //            new Expression[] { source1.Expression, GetSourceExpression(source2) }
        //            ));
        //}

        //public static QueryableDataSet<TSource> Intersect<TSource>(this QueryableDataSet<TSource> source1, IEnumerable<TSource> source2, IEqualityComparer<TSource> comparer)
        //{
        //    return (QueryableDataSet<TSource>)source1.InternalProvider.CreateQuery<TSource>(
        //        Expression.Call(
        //            null,
        //            GetMethodInfoOf(() => Queryable.Intersect(
        //                default(QueryableDataSet<TSource>),
        //                default(IEnumerable<TSource>),
        //                default(IEqualityComparer<TSource>))),
        //            new Expression[] {
        //                source1.Expression,
        //                GetSourceExpression(source2),
        //                Expression.Constant(comparer, typeof(IEqualityComparer<TSource>))
        //                }
        //            ));
        //}

        //public static QueryableDataSet<TSource> Except<TSource>(this QueryableDataSet<TSource> source1, IEnumerable<TSource> source2)
        //{
        //    return (QueryableDataSet<TSource>)source1.InternalProvider.CreateQuery<TSource>(
        //        Expression.Call(
        //            null,
        //            GetMethodInfoOf(() => Queryable.Except(
        //                default(QueryableDataSet<TSource>),
        //                default(IEnumerable<TSource>))),
        //            new Expression[] { source1.Expression, GetSourceExpression(source2) }
        //            ));
        //}

        //public static QueryableDataSet<TSource> Except<TSource>(this QueryableDataSet<TSource> source1, IEnumerable<TSource> source2, IEqualityComparer<TSource> comparer)
        //{
        //    return (QueryableDataSet<TSource>)source1.InternalProvider.CreateQuery<TSource>(
        //        Expression.Call(
        //            null,
        //            GetMethodInfoOf(() => Queryable.Except(
        //                default(QueryableDataSet<TSource>),
        //                default(IEnumerable<TSource>),
        //                default(IEqualityComparer<TSource>))),
        //            new Expression[] {
        //                source1.Expression,
        //                GetSourceExpression(source2),
        //                Expression.Constant(comparer, typeof(IEqualityComparer<TSource>))
        //                }
        //            ));
        //}

        public static TSource First<TSource>(this QueryableDataSet<TSource> source)
        {
            return source.InternalProvider.Execute<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.First(
                        default(QueryableDataSet<TSource>))),
                    new Expression[] { source.Expression }
                    ));
        }

        public static TSource First<TSource>(this QueryableDataSet<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            return source.InternalProvider.Execute<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.First(
                        default(QueryableDataSet<TSource>),
                        default(Expression<Func<TSource, bool>>))),
                    new Expression[] { source.Expression, Expression.Quote(predicate) }
                    ));
        }

        public static TSource FirstOrDefault<TSource>(this QueryableDataSet<TSource> source)
        {
            MethodInfo method = null;
            if (source.Expression.Type == typeof(TSource))
                method = method = GetMethodInfoOf(() => QueryableDataSetExtensions.FirstOrDefault(
                        default(QueryableDataSet<TSource>)));
            else
                method = GetMethodInfoOf(Queryable.FirstOrDefault, source);

            var exp = Expression.Call(null, method, new Expression[] { source.Expression });
            return source.InternalProvider.Execute<TSource>(exp);
        }

        public static TSource FirstOrDefault<TSource>(this QueryableDataSet<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            return source.InternalProvider.Execute<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.FirstOrDefault(
                        default(QueryableDataSet<TSource>),
                        default(Expression<Func<TSource, bool>>))),
                    new Expression[] { source.Expression, Expression.Quote(predicate) }
                    ));
        }

        public static TSource Last<TSource>(this QueryableDataSet<TSource> source)
        {
            return source.InternalProvider.Execute<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.Last(
                        default(QueryableDataSet<TSource>))),
                    new Expression[] { source.Expression }
                    ));
        }

        public static TSource Last<TSource>(this QueryableDataSet<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            return source.InternalProvider.Execute<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.Last(
                        default(QueryableDataSet<TSource>),
                        default(Expression<Func<TSource, bool>>))),
                    new Expression[] { source.Expression, Expression.Quote(predicate) }
                    ));
        }

        public static TSource LastOrDefault<TSource>(this QueryableDataSet<TSource> source)
        {
            return source.InternalProvider.Execute<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.LastOrDefault(
                        default(QueryableDataSet<TSource>))),
                    new Expression[] { source.Expression }
                    ));
        }

        public static TSource LastOrDefault<TSource>(this QueryableDataSet<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            return source.InternalProvider.Execute<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.LastOrDefault(
                        default(QueryableDataSet<TSource>),
                        default(Expression<Func<TSource, bool>>))),
                    new Expression[] { source.Expression, Expression.Quote(predicate) }
                    ));
        }

        public static TSource Single<TSource>(this QueryableDataSet<TSource> source)
        {
            return source.InternalProvider.Execute<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.Single(
                        default(QueryableDataSet<TSource>))),
                    new Expression[] { source.Expression }
                    ));
        }

        public static TSource Single<TSource>(this QueryableDataSet<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            return source.InternalProvider.Execute<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.Single(
                        default(QueryableDataSet<TSource>),
                        default(Expression<Func<TSource, bool>>))),
                    new Expression[] { source.Expression, Expression.Quote(predicate) }
                    ));
        }

        public static TSource SingleOrDefault<TSource>(this QueryableDataSet<TSource> source)
        {
            return source.InternalProvider.Execute<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.SingleOrDefault(
                        default(QueryableDataSet<TSource>))),
                    new Expression[] { source.Expression }
                    ));
        }

        public static TSource SingleOrDefault<TSource>(this QueryableDataSet<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            return source.InternalProvider.Execute<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.SingleOrDefault(
                        default(QueryableDataSet<TSource>),
                        default(Expression<Func<TSource, bool>>))),
                    new Expression[] { source.Expression, Expression.Quote(predicate) }
                    ));
        }

        public static TSource ElementAt<TSource>(this QueryableDataSet<TSource> source, int index)
        {
            return source.InternalProvider.Execute<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.ElementAt(
                        default(QueryableDataSet<TSource>),
                        default(int))),
                    new Expression[] { source.Expression, Expression.Constant(index) }
                    ));
        }

        public static TSource ElementAtOrDefault<TSource>(this QueryableDataSet<TSource> source, int index)
        {
            return source.InternalProvider.Execute<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.ElementAtOrDefault(
                        default(QueryableDataSet<TSource>),
                        default(int))),
                    new Expression[] { source.Expression, Expression.Constant(index) }
                    ));
        }


        public static bool Contains<TSource>(this QueryableDataSet<TSource> source, TSource item)
        {
            return source.InternalProvider.Execute<bool>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.Contains(
                        default(QueryableDataSet<TSource>),
                        default(TSource))),
                    new Expression[] { source.Expression, Expression.Constant(item, typeof(TSource)) }
                    ));
        }

        public static bool Contains<TSource>(this QueryableDataSet<TSource> source, TSource item, IEqualityComparer<TSource> comparer)
        {
            return source.InternalProvider.Execute<bool>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.Contains(
                        default(QueryableDataSet<TSource>),
                        default(TSource),
                        default(IEqualityComparer<TSource>))),
                    new Expression[] { source.Expression, Expression.Constant(item, typeof(TSource)), Expression.Constant(comparer, typeof(IEqualityComparer<TSource>)) }
                    ));
        }

        public static QueryableDataSet<TSource> Reverse<TSource>(this QueryableDataSet<TSource> source)
        {
            return (QueryableDataSet<TSource>)source.InternalProvider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.Reverse(
                        default(QueryableDataSet<TSource>))),
                    new Expression[] { source.Expression }
                    ));
        }

        public static bool Any<TSource>(this QueryableDataSet<TSource> source)
        {
            return source.InternalProvider.Execute<bool>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.Any(
                        default(QueryableDataSet<TSource>))),
                    new Expression[] { source.Expression }
                    ));
        }

        public static bool Any<TSource>(this QueryableDataSet<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            return source.InternalProvider.Execute<bool>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.Any(
                        default(QueryableDataSet<TSource>),
                        default(Expression<Func<TSource, bool>>))),
                    new Expression[] { source.Expression, Expression.Quote(predicate) }
                    ));
        }

        public static bool All<TSource>(this QueryableDataSet<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            return source.InternalProvider.Execute<bool>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.All(
                        default(QueryableDataSet<TSource>),
                        default(Expression<Func<TSource, bool>>))),
                    new Expression[] { source.Expression, Expression.Quote(predicate) }
                    ));
        }
        public static int Count<TKey, TElement>(this OGrouping<TKey, TElement> source)
        {
            return source.Count;
        }
        public static int Count<TSource>(this QueryableDataSet<TSource> source)
        {
            return source.InternalProvider.Execute<int>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.Count(
                        default(QueryableDataSet<TSource>))),
                    new Expression[] { source.Expression }
                    ));
        }
        public static int Count(this QueryableDataSet source)
        {
            return source.InternalProvider.Execute<int>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.Count(
                        default(QueryableDataSet))),
                    new Expression[] { source.Expression }
                    ));
        }

        public static int Count<TSource>(this QueryableDataSet<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            return source.InternalProvider.Execute<int>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.Count(
                        default(QueryableDataSet<TSource>),
                        default(Expression<Func<TSource, bool>>))),
                    new Expression[] { source.Expression, Expression.Quote(predicate) }
                    ));
        }

        public static long LongCount<TSource>(this QueryableDataSet<TSource> source)
        {
            return source.InternalProvider.Execute<long>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.LongCount(
                        default(QueryableDataSet<TSource>))),
                    new Expression[] { source.Expression }
                    ));
        }

        public static long LongCount<TSource>(this QueryableDataSet<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            return source.InternalProvider.Execute<long>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.LongCount(
                        default(QueryableDataSet<TSource>),
                        default(Expression<Func<TSource, bool>>))),
                    new Expression[] { source.Expression, Expression.Quote(predicate) }
                    ));
        }

        public static TSource Min<TSource>(this QueryableDataSet<TSource> source)
        {
            return source.InternalProvider.Execute<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.Min(
                        default(QueryableDataSet<TSource>))),
                    new Expression[] { source.Expression }
                    ));
        }

        public static TResult Min<TSource, TResult>(this QueryableDataSet<TSource> source, Expression<Func<TSource, TResult>> selector)
        {
            return source.InternalProvider.Execute<TResult>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.Min(
                        default(QueryableDataSet<TSource>),
                        default(Expression<Func<TSource, TResult>>))),
                    new Expression[] { source.Expression, Expression.Quote(selector) }
                    ));
        }

        public static TSource Max<TSource>(this QueryableDataSet<TSource> source)
        {
            return source.InternalProvider.Execute<TSource>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.Max(
                        default(QueryableDataSet<TSource>))),
                    new Expression[] { source.Expression }
                    ));
        }

        public static TResult Max<TSource, TResult>(this QueryableDataSet<TSource> source, Expression<Func<TSource, TResult>> selector)
        {
            return source.InternalProvider.Execute<TResult>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.Max(
                        default(QueryableDataSet<TSource>),
                        default(Expression<Func<TSource, TResult>>))),
                    new Expression[] { source.Expression, Expression.Quote(selector) }
                    ));
        }

        public static TResult Sum<TSource, TResult>(this QueryableDataSet<TSource> source, Expression<Func<TSource, TResult>> selector)
        {
            return source.InternalProvider.Execute<TResult>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.Sum(
                        default(QueryableDataSet<TSource>),
                        default(Expression<Func<TSource, TResult>>))),
                    new Expression[] { source.Expression, Expression.Quote(selector) }
                    ));
        }

        public static TResult Average<TSource, TResult>(this QueryableDataSet<TSource> source, Expression<Func<TSource, TResult>> selector)
        {
            return source.InternalProvider.Execute<TResult>(
                Expression.Call(
                    null,
                    GetMethodInfoOf(() => QueryableDataSetExtensions.Average(
                        default(QueryableDataSet<TSource>),
                        default(Expression<Func<TSource, TResult>>))),
                    new Expression[] { source.Expression, Expression.Quote(selector) }
                    ));
        }
        
        //public static TSource Aggregate<TSource>(this QueryableDataSet<TSource> source, Expression<Func<TSource, TSource, TSource>> func)
        //{
        //    return source.InternalProvider.Execute<TSource>(
        //        Expression.Call(
        //            null,
        //            GetMethodInfoOf(() => Queryable.Aggregate(
        //                default(QueryableDataSet<TSource>),
        //                default(Expression<Func<TSource, TSource, TSource>>))),
        //            new Expression[] { source.Expression, Expression.Quote(func) }
        //            ));
        //}

        //public static TAccumulate Aggregate<TSource, TAccumulate>(this QueryableDataSet<TSource> source, TAccumulate seed, Expression<Func<TAccumulate, TSource, TAccumulate>> func)
        //{
        //    return source.InternalProvider.Execute<TAccumulate>(
        //        Expression.Call(
        //            null,
        //            GetMethodInfoOf(() => Queryable.Aggregate(
        //                default(QueryableDataSet<TSource>),
        //                default(TAccumulate),
        //                default(Expression<Func<TAccumulate, TSource, TAccumulate>>))),
        //            new Expression[] { source.Expression, Expression.Constant(seed), Expression.Quote(func) }
        //            ));
        //}

        //public static TResult Aggregate<TSource, TAccumulate, TResult>(this QueryableDataSet<TSource> source, TAccumulate seed, Expression<Func<TAccumulate, TSource, TAccumulate>> func, Expression<Func<TAccumulate, TResult>> selector)
        //{
        //    return source.InternalProvider.Execute<TResult>(
        //        Expression.Call(
        //            null,
        //            GetMethodInfoOf(() => Queryable.Aggregate(
        //                default(QueryableDataSet<TSource>),
        //                default(TAccumulate),
        //                default(Expression<Func<TAccumulate, TSource, TAccumulate>>),
        //                default(Expression<Func<TAccumulate, TResult>>))),
        //            new Expression[] { source.Expression, Expression.Constant(seed), Expression.Quote(func), Expression.Quote(selector) }
        //            ));
        //}
        public static object Update<TSource, TProperty>(this QueryableDataSet<TSource> source, Expression<Func<TSource, TProperty>> predicate, object value)
        {
            return source.Context.CreateUpdateQuery(source.Expression as MethodCallExpression, source, new Expressions.UpdateExpression(predicate, value)).Execute<int>();
        }

        public static object Delete<TSource>(this QueryableDataSet<TSource> source)
        {
            return source.Context.CreateDeleteQuery(source.Expression as MethodCallExpression, source).Execute<int>();
        }

        private static MethodInfo GetMethodInfoOf<T>(Expression<Func<T>> expression)
        {
            var body = (MethodCallExpression)expression.Body;
            return body.Method;
        }
        private static MethodInfo GetMethodInfoOf<T1, T2>(Func<T1, T2> f, T1 unused1)
        {
            return f.Method;
        }
    }
}
