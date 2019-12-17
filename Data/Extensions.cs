using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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
    }
}
