using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Ophelia
{
    public static class ExpressionExtensions
    {
        public static Expression RemoveConvert(this Expression expression)
        {
            if (expression == null)
                return expression;

            while (expression.NodeType == ExpressionType.Convert
                   || expression.NodeType == ExpressionType.ConvertChecked)
            {
                expression = ((UnaryExpression)expression).Operand;
            }

            return expression;
        }
        //public static string ParsePath<T>(this Func<T, object> func)
        //{
        //    return (func as LambdaExpression).ParsePath();
        //}
        public static Expression<Func<T, object>> GetPropertyExpression<T>(this Type Type, string property)
        {
            var parameter = Expression.Parameter(Type);
            var body = Expression.Property(parameter, property);
            return Expression.Lambda<Func<T, object>>(body, parameter);
        }
        public static string ParsePath(this Expression expression)
        {
            var path = "";
            expression.TryParsePath(out path);
            return path;
        }

        public static object Execute<T>(this Func<T, object> func, T item)
        {
            return func.Invoke(item);
        }
        public static Type GetPropertyType(this Expression expression)
        {
            if (expression is MemberExpression)
            {
                var memberExpression = expression as MemberExpression;
                if (memberExpression != null)
                {
                    if (memberExpression.Member is System.Reflection.PropertyInfo)
                    {
                        var property = memberExpression.Member as System.Reflection.PropertyInfo;
                        return property.PropertyType;
                    }
                }
            }
            else if (expression is LambdaExpression)
            {
                var lambda = expression as LambdaExpression;
                var unaryExp = lambda.Body as UnaryExpression;
                if (unaryExp != null)
                {
                    return (unaryExp.Operand as MemberExpression).GetPropertyType();
                }
                var memberExp = lambda.Body as MemberExpression;
                if (memberExp != null)
                {
                    return memberExp.GetPropertyType();
                }
            }
            return null;
        }
        public static object GetValue(this Expression expression, object item, int languageID = 0, string[] excludedProps = null, Reflection.Accessor.NullReferenceEventDelegate nullReferenceEventDelegate = null)
        {
            if (item == null || expression == null)
                return null;

            var methodCallExpression = expression as MethodCallExpression;
            if (methodCallExpression == null && expression is LambdaExpression)
            {
                methodCallExpression = (expression as LambdaExpression).Body as MethodCallExpression;
            }

            var path = "";
            expression.TryParsePath(out path);
            if (string.IsNullOrEmpty(path))
                return null;

            var accessor = new Ophelia.Reflection.Accessor();
            if (languageID > 0)
            {
                bool isDefaultProp = false;

                if (!isDefaultProp)
                {
                    isDefaultProp = excludedProps != null && (excludedProps.Contains(path) || path == "LanguageID");
                    if (!isDefaultProp)
                    {
                        var i18nProp = item.GetType().GetProperty("I18n");
                        if (i18nProp == null)
                            i18nProp = item.GetType().GetProperty(item.GetType().Name + "_i18n");

                        if (i18nProp != null)
                        {
                            var isExcluded = false;
                            var defaultColumnExcludedProps = i18nProp.PropertyType.UnderlyingSystemType.GenericTypeArguments[0].GetCustomAttributes().Where(op => op.GetType().Name == "ExcludeDefaultColumn").ToList();
                            if (defaultColumnExcludedProps != null)
                            {
                                if (defaultColumnExcludedProps.Where(op => op.GetPropertyValue("Columns").ExecuteMethod("Contains", path).Equals(true)).Any())
                                {
                                    isExcluded = true;
                                }
                            }
                            if (!isExcluded)
                            {
                                if (methodCallExpression == null && i18nProp != null && i18nProp.PropertyType.UnderlyingSystemType.GenericTypeArguments[0].GetProperty(path) != null)
                                {
                                    var list = (IEnumerable)i18nProp.GetValue(item, null);
                                    if (list != null)
                                    {
                                        foreach (object i18n in list)
                                        {
                                            if (Convert.ToInt32(i18n.GetType().GetProperty("LanguageID").GetValue(i18n, null)) == languageID)
                                            {
                                                accessor.Item = i18n;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (accessor.Item == null)
                accessor.Item = item;
            accessor.MemberName = path;
            accessor.MethodCallExpression = methodCallExpression;
            accessor.NullReferenceEventHandler += nullReferenceEventDelegate;
            var value = accessor.Value;
            accessor.NullReferenceEventHandler -= nullReferenceEventDelegate;
            return value;
        }

        public static bool TryParsePath(this Expression expression, out string path)
        {
            path = null;
            var withoutConvert = expression.RemoveConvert(); // Removes boxing
            var memberExpression = withoutConvert as MemberExpression;
            var callExpression = withoutConvert as MethodCallExpression;
            var lambdaExpression = withoutConvert as LambdaExpression;

            if (memberExpression != null)
            {
                var thisPart = memberExpression.Member.Name;
                string parentPart;
                if (!TryParsePath(memberExpression.Expression, out parentPart))
                {
                    return false;
                }
                path = parentPart == null ? thisPart : (parentPart + "." + thisPart);
            }
            else if (lambdaExpression != null)
            {
                memberExpression = lambdaExpression.Body as MemberExpression;
                var unaryExpression = lambdaExpression.Body as UnaryExpression;
                var methodCallExpression = lambdaExpression.Body as MethodCallExpression;

                if (methodCallExpression != null)
                {
                    path = methodCallExpression.ToString();
                    path = path.Right(path.Length - path.IndexOf(".") - 1);
                    return true;
                }
                if (memberExpression != null)
                {
                    var thisPart = memberExpression.Member.Name;
                    string parentPart;
                    if (!TryParsePath(memberExpression.Expression, out parentPart))
                    {
                        return false;
                    }
                    path = parentPart == null ? thisPart : (parentPart + "." + thisPart);
                }
                else if (unaryExpression != null)
                {
                    string thisPart;
                    if (!TryParsePath(unaryExpression.Operand, out thisPart))
                    {
                        return false;
                    }
                    path = thisPart;
                }
            }
            else if (callExpression != null)
            {
                if (callExpression.Method.Name == "Select"
                    && callExpression.Arguments.Count == 2)
                {
                    string parentPart;
                    if (!TryParsePath(callExpression.Arguments[0], out parentPart))
                    {
                        return false;
                    }
                    if (parentPart != null)
                    {
                        var subExpression = callExpression.Arguments[1] as LambdaExpression;
                        if (subExpression != null)
                        {
                            string thisPart;
                            if (!TryParsePath(subExpression.Body, out thisPart))
                            {
                                return false;
                            }
                            if (thisPart != null)
                            {
                                path = parentPart + "." + thisPart;
                                return true;
                            }
                        }

                        var unaryExpression = callExpression.Arguments[1] as UnaryExpression;
                        if (unaryExpression != null)
                        {
                            string thisPart;
                            if (!TryParsePath((unaryExpression.Operand as LambdaExpression).Body, out thisPart))
                            {
                                return false;
                            }
                            if (thisPart != null)
                            {
                                path = parentPart + ".{" + thisPart + "}";
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    var tmp = callExpression.ToString();
                    path = tmp.Right(tmp.Length - tmp.IndexOf(".") - 1);
                    tmp = "";
                    if (path.IndexOf("FirstOrDefault()") > -1)
                        path = path.Replace(".FirstOrDefault()", "[0]");
                    return true;
                }
                return false;
            }

            return true;
        }

        public static Expression<Func<TInput, bool>> CombineWithAndAlso<TInput>(this Expression<Func<TInput, bool>> func1, Expression<Func<TInput, bool>> func2)
        {
            return Expression.Lambda<Func<TInput, bool>>(
                Expression.AndAlso(
                    func1.Body, new ExpressionParameterReplacer(func2.Parameters, func1.Parameters).Visit(func2.Body)),
                func1.Parameters);
        }

        public static Expression<Func<TInput, bool>> CombineWithOrElse<TInput>(this Expression<Func<TInput, bool>> func1, Expression<Func<TInput, bool>> func2)
        {
            return Expression.Lambda<Func<TInput, bool>>(
                Expression.OrElse(
                    func1.Body, new ExpressionParameterReplacer(func2.Parameters, func1.Parameters).Visit(func2.Body)),
                func1.Parameters);
        }

        private class ExpressionParameterReplacer : ExpressionVisitor
        {
            public ExpressionParameterReplacer(IList<ParameterExpression> fromParameters, IList<ParameterExpression> toParameters)
            {
                ParameterReplacements = new Dictionary<ParameterExpression, ParameterExpression>();
                for (int i = 0; i != fromParameters.Count && i != toParameters.Count; i++)
                    ParameterReplacements.Add(fromParameters[i], toParameters[i]);
            }

            private IDictionary<ParameterExpression, ParameterExpression> ParameterReplacements { get; set; }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                ParameterExpression replacement;
                if (ParameterReplacements.TryGetValue(node, out replacement))
                    node = replacement;
                return base.VisitParameter(node);
            }
        }
    }
}
