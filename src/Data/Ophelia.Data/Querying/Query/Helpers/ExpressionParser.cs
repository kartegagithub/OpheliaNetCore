using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Ophelia.Data.Querying.Query.Helpers
{
    public class ExpressionParser : IDisposable
    {
        public ExpressionParser? Left { get; set; }
        public ExpressionParser? Right { get; set; }
        public ExpressionParser? SubExpression { get; set; }
        public PropertyInfo? PropertyInfo { get; set; }
        public PropertyInfo? ParentPropertyInfo { get; set; }
        public object? Value { get; set; }
        public object? Value2 { get; set; }
        public string Name { get; set; } = "";
        public bool IsLogicalExpression { get; set; }
        public Constraint Constraint { get; set; }
        public Comparison Comparison { get; set; }
        public bool Exclude { get; set; }
        public Type? EntityType { get; set; }
        public bool IsQueryableDataSet { get; set; }
        public bool IsDataEntity { get; set; }
        public int Take { get; set; }
        public int Skip { get; set; }
        public bool ParameterValueIsInLeftSide { get; set; }
        public bool IsIncluder { get; set; }
        public bool IsSubInclude { get; set; }
        public List<MemberInfo> Members { get; set; } = new List<MemberInfo>();
        public Dictionary<MemberInfo, Expression> BindingMembers { get; set; } = new Dictionary<MemberInfo, Expression>();
        public List<Expression> MemberExpressions { get; set; } = new List<Expression>();
        public static ExpressionParser Create(System.Linq.Expressions.Expression expression)
        {
            ExpressionParser exp = null;
            if (expression is LambdaExpression)
            {
                var callExpression = (expression as LambdaExpression).Body as MethodCallExpression;
                if (callExpression != null)
                    exp = ExpressionParser.Parse(callExpression);

                var binaryExpression = (expression as LambdaExpression).Body as BinaryExpression;
                if (binaryExpression != null)
                {
                    exp = ExpressionParser.Parse(binaryExpression);
                }

                var unaryExpression = (expression as LambdaExpression).Body as UnaryExpression;
                if (unaryExpression != null)
                {
                    exp = ExpressionParser.Parse(unaryExpression);
                }

                var memberExpression = (expression as LambdaExpression).Body as MemberExpression;
                if (memberExpression != null)
                {
                    exp = ExpressionParser.Parse(memberExpression);
                }
                var newExpression = (expression as LambdaExpression).Body as NewExpression;
                if (newExpression != null)
                {
                    exp = ExpressionParser.Parse(newExpression);
                }

                var memberInitExpression = (expression as LambdaExpression).Body as MemberInitExpression;
                if (memberInitExpression != null)
                {
                    exp = ExpressionParser.Parse(memberInitExpression);
                }
            }
            else if (expression is BinaryExpression)
            {
                exp = ExpressionParser.Parse(expression as BinaryExpression);
            }
            else if (expression is MethodCallExpression)
            {
                exp = ExpressionParser.Parse(expression as MethodCallExpression);
            }
            else if (expression is UnaryExpression)
            {
                exp = ExpressionParser.Parse(expression as UnaryExpression);
            }
            else if (expression is MemberExpression)
            {
                exp = ExpressionParser.Parse(expression as MemberExpression);
            }
            else if (expression is Expressions.InExpression)
            {
                exp = ExpressionParser.Parse(expression as Expressions.InExpression);
            }
            else if (expression is ConstantExpression)
            {
                exp = ExpressionParser.Parse(expression as ConstantExpression);
            }
            return exp;
        }
        public static ExpressionParser Parse(MemberInitExpression expression)
        {
            var parser = new ExpressionParser();
            parser.Members = new List<MemberInfo>();
            if (expression.NewExpression == null)
            {
                foreach (var item in expression.Bindings)
                {
                    parser.Members.Add(item.Member);
                }
            }
            else
            {
                parser.BindingMembers = new Dictionary<MemberInfo, Expression>();
                parser.EntityType = expression.Type;
                foreach (var item in expression.Bindings)
                {
                    if (item.BindingType == MemberBindingType.Assignment)
                    {
                        var memberAssignment = (item as MemberAssignment);
                        if (memberAssignment.Expression is MemberExpression)
                        {
                            parser.Members.Add((memberAssignment.Expression as MemberExpression).Member);
                            parser.BindingMembers.Add(item.Member, memberAssignment.Expression);
                        }
                        else if (memberAssignment.Expression is MemberInitExpression)
                        {
                            parser.Members.Add(item.Member);
                            parser.BindingMembers.Add(item.Member, memberAssignment.Expression);
                        }
                        else if (memberAssignment.Expression is ConstantExpression)
                        {
                            parser.Members.Add(item.Member);
                            parser.BindingMembers.Add(item.Member, memberAssignment.Expression);
                        }
                        else if (memberAssignment.Expression is UnaryExpression)
                        {
                            var unary = memberAssignment.Expression as UnaryExpression;
                            if (unary.Operand is MemberExpression)
                            {
                                var memberExp = unary.Operand as MemberExpression;
                                if (memberAssignment != null)
                                    parser.Members.Add(memberAssignment.Member);
                                else
                                    parser.Members.Add(memberExp.Member);
                                parser.BindingMembers.Add(memberExp.Member, unary.Operand);
                            }
                        }
                    }
                }
            }
            return parser;
        }
        public static ExpressionParser Parse(ConstantExpression expression)
        {
            var parser = new ExpressionParser();
            parser.Value = expression.Value;
            return parser;
        }
        public static ExpressionParser Parse(Expressions.InExpression expression)
        {
            var parser = new ExpressionParser();
            parser.Comparison = Comparison.Exists;
            parser.Value = (expression.Expression as ConstantExpression).Value;
            parser.Value2 = expression.Relation;
            parser.IsQueryableDataSet = true;
            parser.EntityType = parser.Value.GetType().GetGenericArguments()[0];
            parser.Comparison = Comparison.Exists;
            return parser;
        }
        public static ExpressionParser Parse(NewExpression expression)
        {
            var parser = new ExpressionParser();
            parser.Members = new List<MemberInfo>();
            parser.BindingMembers = new Dictionary<MemberInfo, Expression>();
            var counter = 0;
            foreach (var exp in expression.Arguments)
            {
                parser.Members.Add((exp as MemberExpression).Member);
                parser.BindingMembers.Add(expression.Members[counter], exp);
                counter++;
            }
            return parser;
        }
        public static ExpressionParser Parse(BinaryExpression expression)
        {
            var parser = new ExpressionParser();
            if (expression.GetType().Name.Contains("SimpleBinary", StringComparison.InvariantCultureIgnoreCase))
                parser.IsLogicalExpression = true;
            if (parser.IsLogicalExpression)
            {
                if (expression.Left != null)
                {
                    parser.Left = ExpressionParser.Create(expression.Left);
                    parser.Left.Comparison = Comparison.None;
                    parser.Left.IsLogicalExpression = parser.IsLogicalExpression;
                    if ((expression.Left is UnaryExpression))
                        parser.Left.Exclude = (expression.Left as UnaryExpression).NodeType == ExpressionType.Not;
                    if (parser.Left.Value != null)
                    {
                        parser.Left.Name = "";
                        parser.Left.PropertyInfo = null;
                    }
                }
                if (expression.Right != null)
                {
                    parser.Right = ExpressionParser.Create(expression.Right);
                    parser.Right.Comparison = Comparison.None;
                    parser.Right.IsLogicalExpression = parser.IsLogicalExpression;
                    if (parser.Right.Value != null)
                    {
                        parser.Right.Name = "";
                        parser.Right.PropertyInfo = null;
                    }
                }
            }
            else
            {
                if (!(expression.Left is MemberExpression))
                {
                    if (expression.Left is UnaryExpression)
                    {
                        if ((expression.Left as UnaryExpression).Operand is MemberExpression)
                        {
                            var memberExpression = (expression.Left as UnaryExpression).Operand as MemberExpression;
                            parser.Name = memberExpression.ParsePath();
                        }
                        else
                        {
                            parser.Left = ExpressionParser.Create((expression.Left as UnaryExpression).Operand);
                            parser.Left.Exclude = (expression.Left as UnaryExpression).NodeType == ExpressionType.Not;
                        }
                    }
                    else
                    {
                        parser.Left = ExpressionParser.Create(expression.Left);
                        if (parser.Left == null)
                            throw new Exception("Left Expression is null");
                    }
                }
                else if ((expression.Left as MemberExpression).CanGetExpressionValue(expression))
                {
                    parser.Value = (expression.Left as MemberExpression).GetExpressionValue(expression);
                    parser.ParameterValueIsInLeftSide = true;
                }
                else
                {
                    //TODO: op.IsBoolParameter & !op.Bool2Parameter fails.
                    parser.Name = expression.Left.ParsePath();
                }
                if (expression.Right is MemberExpression && (expression.Right as MemberExpression).Expression is ConstantExpression)
                {
                    var memberExpression = (expression.Right as MemberExpression);
                    if (memberExpression.Member is System.Reflection.FieldInfo)
                    {
                        var field = memberExpression.Member as System.Reflection.FieldInfo;
                        if (field.FieldType.IsQueryableDataSet() || field.FieldType.IsQueryable())
                        {
                            parser.Value = field.FieldType;
                            parser.IsQueryableDataSet = true;
                            parser.EntityType = field.FieldType.GetGenericArguments()[0];
                            parser.Comparison = Comparison.Exists;
                        }
                        else if (field.FieldType.IsDataEntity() || field.FieldType.IsPOCOEntity())
                        {
                            parser.IsDataEntity = true;
                            parser.EntityType = field.FieldType;
                        }
                        else
                        {
                            parser.Value = field.GetValue((memberExpression.Expression as ConstantExpression).Value);
                        }
                    }
                    else if (memberExpression.Member is System.Reflection.PropertyInfo)
                    {
                        var property = memberExpression.Member as System.Reflection.PropertyInfo;
                        if (property.PropertyType.IsQueryableDataSet() || property.PropertyType.IsQueryable())
                        {
                            parser.Value = property.PropertyType;
                            parser.IsQueryableDataSet = true;
                            parser.EntityType = property.PropertyType.GetGenericArguments()[0];
                            parser.Comparison = Comparison.Exists;
                        }
                        else if (property.PropertyType.IsDataEntity() || property.PropertyType.IsPOCOEntity())
                        {
                            parser.IsDataEntity = true;
                            parser.EntityType = property.PropertyType;
                        }
                        else
                        {
                            parser.Value = property.GetValue((memberExpression.Expression as ConstantExpression).Value);
                        }
                    }
                }
                else if (expression.Right is MemberExpression && (expression.Right as MemberExpression).Expression is MemberExpression && ((expression.Right as MemberExpression).Expression as MemberExpression).Expression is ConstantExpression)
                {
                    parser.Value = Expression.Lambda(expression.Right).Compile().DynamicInvoke();
                }
                else if (expression.Right is UnaryExpression && expression.Right.NodeType == ExpressionType.Convert)
                {
                    var memberExpression = (expression.Right as UnaryExpression).Operand as MemberExpression;
                    if (memberExpression != null)
                    {
                        parser.Value = memberExpression.GetExpressionValue(expression);
                    }
                    var constantExpression = (expression.Right as UnaryExpression).Operand as ConstantExpression;
                    if (constantExpression != null)
                    {
                        parser.Value = constantExpression.Value;
                    }
                    var unaryExpression = (expression.Right as UnaryExpression).Operand as UnaryExpression;
                    if (unaryExpression != null)
                    {
                        memberExpression = unaryExpression.Operand as MemberExpression;
                        if (memberExpression != null)
                        {
                            parser.Value = memberExpression.GetExpressionValue(expression);
                        }
                    }
                }
                else if (!(expression.Right is ConstantExpression))
                {
                    bool foundValue = false;
                    if (expression.Right is MemberExpression)
                    {
                        var memberExp = (expression.Right as MemberExpression);
                        if (memberExp.Expression == null || memberExp.Expression.NodeType != ExpressionType.Parameter)
                        {
                            parser.Value = memberExp.GetExpressionValue(expression);
                            foundValue = true;
                        }
                    }
                    if (!foundValue)
                    {
                        if (parser.ParameterValueIsInLeftSide)
                        {
                            parser.Name = expression.Right.ParsePath();
                        }
                        else
                        {
                            parser.Right = ExpressionParser.Create(expression.Right);
                            if (parser.Right == null)
                                throw new Exception("Right Expression is null");
                        }
                    }
                }
                else
                {
                    if (!(expression.Right is ConstantExpression) || (parser.Left != null && parser.Left.IsLogicalExpression))
                    {
                        parser.Right = ExpressionParser.Create(expression.Right);
                        if ((parser.Left != null && parser.Left.IsLogicalExpression))
                            parser.Right.Comparison = Comparison.None;
                    }
                    else
                    {
                        var consExpression = expression.Right as ConstantExpression;
                        parser.Value = consExpression.Value;
                    }
                }
            }

            if (expression.NodeType == ExpressionType.And || expression.NodeType == ExpressionType.AndAlso)
            {
                parser.Constraint = Constraint.And;
            }
            else if (expression.NodeType == ExpressionType.Or || expression.NodeType == ExpressionType.OrElse)
            {
                parser.Constraint = Constraint.Or;
            }
            else if (expression.NodeType == ExpressionType.GreaterThan)
            {
                parser.Comparison = Comparison.Greater;
            }
            else if (expression.NodeType == ExpressionType.GreaterThanOrEqual)
            {
                parser.Comparison = Comparison.GreaterAndEqual;
            }
            else if (expression.NodeType == ExpressionType.LessThan)
            {
                parser.Comparison = Comparison.Less;
            }
            else if (expression.NodeType == ExpressionType.LessThanOrEqual)
            {
                parser.Comparison = Comparison.LessAndEqual;
            }
            else if (expression.NodeType == ExpressionType.Equal)
            {
                parser.Comparison = Comparison.Equal;
            }
            else if (expression.NodeType == ExpressionType.NotEqual)
            {
                parser.Comparison = Comparison.Different;
            }
            else if (expression.NodeType == ExpressionType.Not)
            {
                parser.Exclude = true;
            }
            if (parser.IsLogicalExpression)
                parser.Comparison = Comparison.None;

            return parser;
        }
        public static ExpressionParser Parse(MemberExpression expression)
        {
            var parser = new ExpressionParser();
            parser.Name = expression.ParsePath();
            var propInfo = expression.Member as System.Reflection.PropertyInfo;
            var field = expression.Member as System.Reflection.FieldInfo;
            if (propInfo != null)
            {
                parser.PropertyInfo = propInfo;
                if (propInfo.PropertyType.IsQueryableDataSet() || propInfo.PropertyType.IsQueryable())
                {
                    parser.Value = propInfo.PropertyType;
                    parser.IsQueryableDataSet = true;
                    parser.EntityType = propInfo.PropertyType.GetGenericArguments()[0];
                    parser.Comparison = Comparison.Exists;
                }
                else if (propInfo.PropertyType.IsDataEntity() || propInfo.PropertyType.IsPOCOEntity())
                {
                    parser.IsDataEntity = true;
                    parser.EntityType = propInfo.PropertyType;
                }
                else
                {
                    if (propInfo.IsStaticProperty())
                        parser.Value = propInfo.GetStaticPropertyValue();
                    else if (propInfo.PropertyType.IsAssignableFrom(typeof(bool)))
                        parser.Value = true;
                    else if (expression.Expression is ConstantExpression)
                        parser.Value = propInfo.GetValue((expression.Expression as ConstantExpression).Value);
                    else if (expression.Expression is MemberExpression)
                    {
                        var memberExp = expression.Expression as MemberExpression;

                        var propInfo1 = memberExp.Member as System.Reflection.PropertyInfo;
                        var field1 = memberExp.Member as System.Reflection.FieldInfo;
                        object value = null;
                        if (memberExp.Expression is ConstantExpression)
                        {
                            if (propInfo1 != null)
                                value = propInfo1.GetValue((memberExp.Expression as ConstantExpression).Value);
                            if (field1 != null)
                                value = field1.GetValue((memberExp.Expression as ConstantExpression).Value);
                        }
                        else if (memberExp.Expression is MemberExpression)
                        {
                            memberExp = memberExp.Expression as MemberExpression;

                            var propInfo2 = memberExp.Member as System.Reflection.PropertyInfo;
                            var field2 = memberExp.Member as System.Reflection.FieldInfo;
                            if (memberExp.Expression is ConstantExpression)
                            {
                                if (propInfo2 != null)
                                    value = propInfo2.GetValue((memberExp.Expression as ConstantExpression).Value);
                                if (field2 != null)
                                    value = field2.GetValue((memberExp.Expression as ConstantExpression).Value);
                            }

                            if (value != null)
                            {
                                if (propInfo1 != null)
                                    value = propInfo1.GetValue(value);
                                if (field1 != null)
                                    value = field1.GetValue(value);
                            }
                        }

                        if (value != null)
                        {
                            if (propInfo != null)
                                parser.Value = propInfo.GetValue(value);
                            if (field != null)
                                parser.Value = field.GetValue(value);
                        }
                    }
                }
            }
            else
            {
                if (field.FieldType.IsQueryableDataSet() || field.FieldType.IsQueryable())
                {
                    parser.Value = field.FieldType;
                    parser.IsQueryableDataSet = true;
                    parser.EntityType = field.FieldType.GetGenericArguments()[0];
                    parser.Comparison = Comparison.Exists;
                }
                else if (field.FieldType.IsDataEntity() || field.FieldType.IsPOCOEntity())
                {
                    parser.IsDataEntity = true;
                    parser.EntityType = field.FieldType;
                }
                else if (expression.Expression is ConstantExpression)
                {
                    parser.Value = field.GetValue((expression.Expression as ConstantExpression).Value);
                }
            }
            return parser;
        }
        //private static object GetFieldValue(ExpressionParser parser, MemberExpression expression, bool setPropInfo = true)
        //{
        //    var propInfo = expression.Member as PropertyInfo;
        //    var field = expression.Member as FieldInfo;
        //    if (setPropInfo)
        //        parser.PropertyInfo = propInfo;

        //    if (expression.Expression != null)
        //    {
        //        if (expression.Expression is ConstantExpression)
        //        {
        //            if (propInfo != null)
        //                return propInfo.GetValue((expression.Expression as ConstantExpression).Value);
        //            else if(field != null)
        //                return field.GetValue((expression.Expression as ConstantExpression).Value);
        //        }
        //        else if (expression.Expression is MemberExpression)
        //        {
        //            var parentExp = expression.Expression as MemberExpression;
        //            if (parentExp.Member is PropertyInfo)
        //                parser.ParentPropertyInfo = parentExp.Member as PropertyInfo;

        //            var value = GetFieldValue(parser, parentExp);
        //            if(propInfo != null)
        //                return propInfo.GetValue(value);
        //            else if (field != null)
        //                return field.GetValue(value);
        //        }
        //    }
        //    else
        //    {
        //        if (propInfo != null)
        //        {
        //            if (propInfo.PropertyType.IsQueryableDataSet() || propInfo.PropertyType.IsQueryable())
        //            {
        //                parser.Value = propInfo.PropertyType;
        //                parser.IsQueryableDataSet = true;
        //                parser.EntityType = propInfo.PropertyType.GetGenericArguments()[0];
        //                parser.Comparison = Comparison.Exists;
        //            }
        //            else if (propInfo.PropertyType.IsDataEntity() || propInfo.PropertyType.IsPOCOEntity())
        //            {
        //                parser.IsDataEntity = true;
        //                parser.EntityType = propInfo.PropertyType;
        //            }
        //            else
        //            {
        //                if (propInfo.IsStaticProperty())
        //                    parser.Value = propInfo.GetStaticPropertyValue();
        //                else if (propInfo.PropertyType.IsAssignableFrom(typeof(bool)))
        //                    parser.Value = true;
        //            }
        //        }
        //        else
        //        {
        //            if (field == null)
        //                return null;

        //            if (field.FieldType.IsQueryableDataSet() || field.FieldType.IsQueryable())
        //            {
        //                parser.Value = field.FieldType;
        //                parser.IsQueryableDataSet = true;
        //                parser.EntityType = field.FieldType.GetGenericArguments()[0];
        //                parser.Comparison = Comparison.Exists;
        //            }
        //            else if (field.FieldType.IsDataEntity() || field.FieldType.IsPOCOEntity())
        //            {
        //                parser.IsDataEntity = true;
        //                parser.EntityType = field.FieldType;
        //            }
        //            else
        //            {
        //                parser.Value = field.GetValue((expression.Expression as ConstantExpression).Value);
        //            }
        //        }
        //    }
        //    return null;
        //}
        public static ExpressionParser Parse(UnaryExpression expression)
        {
            var parser = new ExpressionParser();
            if (expression.Operand is MethodCallExpression)
            {
                parser = ExpressionParser.Parse(expression.Operand as MethodCallExpression);
                parser.Exclude = expression.NodeType == ExpressionType.Not;
            }
            else if (expression.Operand is MemberExpression)
            {
                parser = ExpressionParser.Parse(expression.Operand as MemberExpression);
                parser.Exclude = expression.NodeType == ExpressionType.Not;
            }
            else if (expression.Operand is LambdaExpression)
            {
                var binaryExpression = ((expression.Operand as LambdaExpression).Body as BinaryExpression);
                if (binaryExpression != null)
                {
                    return Create(binaryExpression);
                }
                else
                {
                    var memberExpression = ((expression.Operand as LambdaExpression).Body as MemberExpression);
                    if (memberExpression != null)
                    {
                        return Create(memberExpression);
                    }
                    else
                    {
                        return Create(expression.Operand);
                    }
                }
            }
            return parser;
        }
        public static ExpressionParser Parse(MethodCallExpression expression)
        {
            var parser = new ExpressionParser();
            if (expression.Method.Name == "StartsWith")
                parser.Comparison = Comparison.StartsWith;
            else if (expression.Method.Name == "Contains")
                parser.Comparison = Comparison.Contains;
            else if (expression.Method.Name == "ContainsFTS")
                parser.Comparison = Comparison.ContainsFTS;
            else if (expression.Method.Name == "EndsWith")
                parser.Comparison = Comparison.EndsWith;
            else if (expression.Method.Name == "Between")
                parser.Comparison = Comparison.Between;
            else if (expression.Method.Name == "Any")
                parser.Comparison = Comparison.Exists;
            else if (expression.Method.Name == "Equals")
                parser.Comparison = Comparison.Equal;
            else if (expression.Method.Name == "Select")
                parser.IsSubInclude = true;
            else if (expression.Method.Name == "Include")
                parser.IsIncluder = true;

            if (parser.Comparison == Comparison.ContainsFTS)
            {
                if (expression.Arguments[0] is MemberExpression)
                    parser.Value = (expression.Arguments[0] as MemberExpression).GetExpressionValue(null);
                else if (expression.Arguments[0] is ConstantExpression)
                    parser.Value = (expression.Arguments[0] as ConstantExpression).Value;
                parser.MemberExpressions = new List<Expression>();
                var expressions = (expression.Arguments[1] as NewArrayExpression).Expressions;
                foreach (var item in expressions)
                {
                    parser.MemberExpressions.Add((item as MemberExpression));
                }
            }
            else if (expression.Object != null)
            {
                if (expression.Object is MemberExpression && !(expression.Object as MemberExpression).Type.IsPrimitiveType() && (expression.Object.GetType().Name.IndexOf("Field") > -1 || expression.Object.GetType().Name.IndexOf("Property") > -1))
                {
                    //idList.Contains(op.Name)
                    parser.Name = expression.Arguments[0].ParsePath();
                    parser.Comparison = Comparison.In;
                    parser.Value = (expression.Object as MemberExpression).GetExpressionValue(null);
                }
                else
                {
                    if (expression.Object is ConstantExpression)
                    {
                        //text.Contains(op.Name)
                        parser.Value = (expression.Object as ConstantExpression).Value;
                        parser.ParameterValueIsInLeftSide = true;
                        if (expression.Arguments[0] is MemberExpression)
                            parser.Name = expression.Arguments[0].ParsePath();
                    }
                    else
                    {
                        // op.Name.Contains("text")
                        parser.Name = expression.Object.ParsePath();
                        if ((expression.Arguments[0] is ConstantExpression))
                            parser.Value = (expression.Arguments[0] as ConstantExpression).Value;
                        else if ((expression.Arguments[0] is MemberExpression))
                            parser.Value = (expression.Arguments[0] as MemberExpression).GetExpressionValue(null);
                    }
                }
            }
            else if (expression.Arguments.Count == 2 && expression.Arguments[0] is ConstantExpression && expression.Arguments[1] is MemberExpression)
            {
                //idList.Contains(op.Name)
                parser.Name = expression.Arguments[1].ParsePath();
                parser.Comparison = Comparison.In;
                parser.Value = (expression.Arguments[0] as ConstantExpression).Value;
            }
            else
            {
                var memberExpression = (expression.Arguments[0] as MemberExpression);
                if (memberExpression != null)
                {
                    parser.Name = memberExpression.ParsePath();
                    if (expression.Arguments.Count > 1)
                    {
                        parser.Name = expression.ParsePath();
                        if ((expression.Arguments[1] is ConstantExpression))
                        {
                            parser.Value = (expression.Arguments[1] as ConstantExpression).Value;
                            if (parser.Comparison == Comparison.Between)
                                parser.Value2 = (expression.Arguments[2] as ConstantExpression).Value;
                        }
                        else
                        {
                            var subExpression = Create(expression.Arguments[1]);
                            parser.SubExpression = subExpression;
                        }
                    }
                    if (memberExpression.Member != null)
                    {
                        var propInfo = memberExpression.Member as System.Reflection.PropertyInfo;
                        parser.PropertyInfo = propInfo;
                        if (propInfo.PropertyType.IsQueryableDataSet() || propInfo.PropertyType.IsQueryable())
                        {
                            parser.Value = propInfo.PropertyType;
                            parser.IsQueryableDataSet = true;
                            parser.EntityType = propInfo.PropertyType.GetGenericArguments()[0];
                            parser.Comparison = Comparison.Exists;
                        }
                        if (propInfo.PropertyType.IsDataEntity() || propInfo.PropertyType.IsPOCOEntity())
                        {
                            parser.IsDataEntity = true;
                            parser.EntityType = propInfo.PropertyType;
                        }
                        if (memberExpression.Expression is MemberExpression)
                        {
                            var parentExp = memberExpression.Expression as MemberExpression;
                            if (parentExp.Member is PropertyInfo)
                                parser.ParentPropertyInfo = parentExp.Member as PropertyInfo;
                        }
                    }
                }
                else if (expression.Arguments[0] is MethodCallExpression)
                {
                    if (expression.Method.Name == "OrderBy" || expression.Method.Name == "OrderByDescending" || expression.Method.Name == "ThenBy" || expression.Method.Name == "ThenByDescending")
                    {
                        if (expression.Arguments.Count > 1 && expression.Arguments[1] is UnaryExpression)
                        {
                            var unaryExp = expression.Arguments[1] as UnaryExpression;
                            if (unaryExp.Operand != null)
                                parser.Name = unaryExp.Operand.ParsePath();
                        }
                    }
                }

                if (expression.Arguments.Count > 1)
                {
                    var consExpression = expression.Arguments[1] as ConstantExpression;
                    if (consExpression != null)
                    {
                        if (expression.Method.Name == "Take")
                            parser.Take = Convert.ToInt32(consExpression.Value);
                        if (expression.Method.Name == "Skip")
                            parser.Skip = Convert.ToInt32(consExpression.Value);
                    }

                    if (expression.Method.Name == "OrderBy" || expression.Method.Name == "OrderByDescending" || expression.Method.Name == "ThenBy" || expression.Method.Name == "ThenByDescending")
                    {
                        if (expression.Arguments[1] is UnaryExpression)
                        {
                            var unaryExp = expression.Arguments[1] as UnaryExpression;
                            if (unaryExp.Operand != null)
                                parser.Name = unaryExp.Operand.ParsePath();
                        }
                    }
                }

                var callExpression = expression.Arguments[0] as MethodCallExpression;
                if (callExpression != null)
                {
                    var subExpression = Create(callExpression);
                    parser.SubExpression = subExpression;
                }
            }
            if (expression.NodeType == ExpressionType.Not)
                parser.Exclude = true;

            return parser;
        }
        public Filter ToFilter()
        {
            var filter = new Filter();
            filter.PropertyInfo = this.PropertyInfo;
            filter.Name = this.Name;
            filter.Constraint = this.Constraint;
            filter.Comparison = this.Comparison;
            filter.IsDataEntity = this.IsDataEntity;
            filter.IsQueryableDataSet = this.IsQueryableDataSet;
            filter.EntityType = this.EntityType;
            filter.IsLogicalExpression = this.IsLogicalExpression;
            filter.Members = this.Members;
            filter.MemberExpressions = this.MemberExpressions;
            filter.ParameterValueIsInLeftSide = this.ParameterValueIsInLeftSide;
            if (this.EntityType != null)
                filter.EntityTypeName = this.EntityType.FullName;

            if (this.SubExpression != null)
            {
                filter.SubFilter = this.SubExpression.ToFilter();
                filter.SubFilter.ParentFilter = filter;
                if (filter.PropertyInfo == null && this.SubExpression.ParentPropertyInfo != null)
                    filter.PropertyInfo = this.SubExpression.ParentPropertyInfo;
            }

            if (this.Left != null)
            {
                filter.Left = this.Left.ToFilter();
                filter.Left.ParentFilter = filter;
            }

            if (this.Right != null)
            {
                filter.Right = this.Right.ToFilter();
                filter.Right.ParentFilter = filter;
            }
            filter.Exclude = this.Exclude;
            filter.Value = this.Value;
            if (this.Value != null)
            {
                filter.ValueType = this.Value.GetType().Name;
                if (this.Value.GetType().IsGenericType)
                    filter.ValueType += "," + this.Value.GetType().GetGenericArguments()[0].Name;
            }
            filter.Value2 = this.Value2;
            if (this.Value2 != null)
            {
                filter.Value2Type = this.Value2.GetType().Name;
                if (this.Value2.GetType().IsGenericType)
                    filter.Value2Type += "," + this.Value2.GetType().GetGenericArguments()[0].Name;
            }
            filter.Take = this.Take;
            filter.Skip = this.Skip;
            return filter;
        }

        public Selector ToSelector()
        {
            var selector = new Selector();
            selector.Name = this.Name;
            selector.PropertyInfo = this.PropertyInfo;
            selector.Members = this.Members;
            selector.BindingMembers = this.BindingMembers;
            if (this.SubExpression != null)
                selector.SubSelector = this.SubExpression.ToSelector();
            return selector;
        }
        public DBFunction ToFunction(string FunctionName, bool IsAggregiate)
        {
            if (this.SubExpression != null)
                return this.SubExpression.ToFunction(FunctionName, IsAggregiate);

            var f = new DBFunction();
            f.FunctionName = FunctionName;
            f.IsAggregiate = IsAggregiate;
            f.Name = this.Name;
            return f;
        }
        public Includer ToIncluder(JoinType joinType)
        {
            var includer = new Includer();
            includer.PropertyInfo = this.PropertyInfo;
            includer.Name = this.Name;
            includer.JoinType = joinType;
            includer.IsDataEntity = this.IsDataEntity;
            includer.IsQueryableDataSet = this.IsQueryableDataSet;
            includer.EntityType = this.EntityType;
            if (this.EntityType != null)
                includer.EntityTypeName = this.EntityType.FullName;
            includer.Constraint = this.Constraint;
            includer.Comparison = this.Comparison;
            includer.PropertyInfo = this.PropertyInfo;
            if (this.SubExpression != null)
            {
                if (this.IsSubInclude)
                    includer.SubIncluders.Add(this.SubExpression.ToIncluder(joinType));
                else if (!this.IsIncluder)
                    includer.SubFilter = this.SubExpression.ToFilter();
            }

            if (this.Left != null)
                includer.Left = this.Left.ToFilter();
            if (this.Right != null)
                includer.Right = this.Right.ToFilter();
            includer.Exclude = this.Exclude;
            includer.Value = this.Value;
            includer.Value2 = this.Value2;
            includer.Take = this.Take;
            includer.Skip = this.Skip;
            if (includer.Take > 0 || includer.Skip > 0 || this.IsIncluder)
            {
                if (this.SubExpression != null)
                    includer.SubIncluders.Add(this.SubExpression.ToIncluder(joinType));
            }
            return includer;
        }

        public Sorter ToSorter(bool Ascending)
        {
            var sorter = new Sorter();
            sorter.Name = this.Name;
            sorter.PropertyInfo = this.PropertyInfo;
            sorter.Ascending = Ascending;
            if (this.SubExpression != null)
                sorter.SubSorter = this.SubExpression.ToSorter(Ascending);
            return sorter;
        }
        public Excluder ToExcluder(bool excludeFromAllQueries)
        {
            var excluder = new Excluder();
            excluder.Name = this.Name;
            excluder.ExcludeFromAllQueries = excludeFromAllQueries;
            if (this.SubExpression != null)
                excluder.SubExcluder = this.SubExpression.ToExcluder(excludeFromAllQueries);
            return excluder;
        }
        public Grouper ToGrouper()
        {
            var grouper = new Grouper();
            grouper.Name = this.Name;
            if (this.EntityType != null)
            {
                grouper.TypeName = this.EntityType.FullName;
                if (this.PropertyInfo != null && (this.PropertyInfo.PropertyType.IsPOCOEntity() || this.PropertyInfo.PropertyType.IsDataEntity()))
                    grouper.Name += "ID";
            }
            grouper.PropertyInfo = this.PropertyInfo;
            grouper.Members = this.Members;
            grouper.BindingMembers = this.BindingMembers;
            if (this.SubExpression != null)
                grouper.SubGrouper = this.SubExpression.ToGrouper();
            return grouper;
        }
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.ParentPropertyInfo = null;
            this.PropertyInfo = null;
            if (this.SubExpression != null)
            {
                this.SubExpression.Dispose();
                this.SubExpression = null;
            }
            if (this.Left != null)
            {
                this.Left.Dispose();
                this.Left = null;
            }
            if (this.Right != null)
            {
                this.Right.Dispose();
                this.Right = null;
            }
            this.EntityType = null;
        }
    }
}
