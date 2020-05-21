using Ophelia.Data.Querying.Query.Helpers;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Ophelia.Data
{
    public static class Extensions
    {
        public static object GetExpressionValue(this MemberExpression memberExpression, BinaryExpression baseExpression)
        {
            if (memberExpression.Expression == null && baseExpression != null)
                return Expression.Lambda(baseExpression.Right).Compile().DynamicInvoke();
            else if (memberExpression.Expression is ConstantExpression)
            {
                var tmpValue = (memberExpression.Expression as ConstantExpression).Value;
                if (tmpValue.GetType().IsValueType || tmpValue.GetType().Name == "String")
                    return tmpValue;
                else
                    return tmpValue.GetPropertyValue(memberExpression.Member.Name);
            }
            else if (memberExpression.Expression is MemberExpression)
            {
                var value = (memberExpression.Expression as MemberExpression).GetExpressionValue(baseExpression);
                if (value.GetType().IsValueType || value.GetType().Name == "String")
                    return value;

                return value.GetPropertyValue(memberExpression.Member.Name);
            }

            var propInfo = memberExpression.Member as System.Reflection.PropertyInfo;
            if (propInfo != null && propInfo.IsStaticProperty())
                return propInfo.GetStaticPropertyValue();

            return Expression.Lambda(memberExpression).Compile().DynamicInvoke();
        }
        public static Model.QueryableDataSet<T> AsQueryableDataSet<T>(this IQueryable<T> query)
        {
            return new Model.QueryableDataSet<T>(query);
        }
        public static Model.QueryableDataSet AsQueryableDataSet(this IQueryable query, Type entityType)
        {
            return (Model.QueryableDataSet)Activator.CreateInstance(typeof(Model.QueryableDataSet<>).MakeGenericType(entityType), new object[] { query });
        }
        public static Model.QueryableDataSet<T> Apply<T>(this Model.QueryableDataSet<T> source, Querying.Query.QueryData data)
        {
            source.ExtendData(data);
            return source;
        }
        public static IQueryable<T> Apply<T>(this IQueryable<T> source, Querying.Query.QueryData data, bool inMemory = false)
        {
            if (data != null)
            {
                if (data.Sorters != null)
                {
                    foreach (var item in data.Sorters)
                    {
                        if (!string.IsNullOrEmpty(item.Name))
                            source = source.OrderBy(item.Name + " " + (item.Ascending ? "asc" : "desc"));
                    }
                }
                if (data.Includers != null)
                {
                    foreach (var item in data.Includers)
                    {
                        if (!string.IsNullOrEmpty(item.Name))
                            source = source.Include(item.Name);
                    }
                }
                if (data.Filter != null)
                {
                    source = source.Apply(data.Filter, inMemory);
                }
            }
            return source;
        }

        public static IQueryable<T> Apply<T>(this IQueryable<T> source, Filter data, bool inMemory = false)
        {
            if (data != null)
            {
                if (data.Left != null)
                    source = source.Apply(data.Left);
                if (data.Right != null)
                    source = source.Apply(data.Right);

                if (data.Left == null && data.Right == null)
                {
                    if (data.SubFilter != null)
                        return source.Apply(data.SubFilter);

                    var applyFilter = true;
                    var comparison = "";
                    switch (data.Comparison)
                    {
                        case Comparison.Equal:
                            comparison = " = @0";
                            break;
                        case Comparison.Different:
                            comparison = " != @0";
                            break;
                        case Comparison.Greater:
                            comparison = " > @0";
                            break;
                        case Comparison.Less:
                            comparison = " < @0";
                            break;
                        case Comparison.GreaterAndEqual:
                            comparison = " >= @0";
                            break;
                        case Comparison.LessAndEqual:
                            comparison = " <= @0";
                            break;
                        case Comparison.In:
                            applyFilter = false;
                            var value = data.ProcessValue(data.Value, data.ValueType);
                            if (value != null)
                                source = source.Where("@0.Contains(outerIt." + data.Name + ")", value);
                            break;
                        case Comparison.Between:
                            break;
                        case Comparison.StartsWith:
                            if (data.Value != null)
                                data.Value = Convert.ToString(data.Value).Trim();
                            comparison = ".ToLower().StartsWith(@0.ToLower())";
                            break;
                        case Comparison.EndsWith:
                            if (data.Value != null)
                                data.Value = Convert.ToString(data.Value).Trim();
                            comparison = ".ToLower().EndsWith(@0.ToLower())";
                            break;
                        case Comparison.Contains:
                            if (data.Value != null)
                                data.Value = Convert.ToString(data.Value).Trim();
                            comparison = ".ToLower().Contains(@0.ToLower())";
                            break;
                        case Comparison.Exists:
                            break;
                        default:
                            break;
                    }
                    if (applyFilter)
                    {
                        if (data.Value != null)
                        {
                            if (data.Value is DateTime dateData)
                            {
                                if (dateData > DateTime.MinValue)
                                    source = source.Where(data.Name + comparison, data.Value);
                            }
                            else if (data.Value is double doubleData)
                            {
                                source = source.Where(data.Name + comparison, decimal.Parse(doubleData.ToString()));
                            }
                            else
                                source = source.Where(data.Name + comparison, data.Value);
                        }
                        else
                            source = source.Where(data.Name + comparison.Replace("@0", "null"));
                    }
                }
            }
            return source;
        }
    }
}
