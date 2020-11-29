using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Ophelia.Data.Querying.Query.Helpers
{
    public class ExpressionParser : IDisposable
    {
        public ExpressionParser Left { get; set; }
        public ExpressionParser Right { get; set; }
        public ExpressionParser SubExpression { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
        public PropertyInfo ParentPropertyInfo { get; set; }
        public object Value { get; set; }
        public object Value2 { get; set; }
        public string Name { get; set; }
        public Constraint Constraint { get; set; }
        public Comparison Comparison { get; set; }
        public bool Exclude { get; set; }
        public Type EntityType { get; set; }
        public bool IsQueryableDataSet { get; set; }
        public bool IsDataEntity { get; set; }
        public int Take { get; set; }
        public int Skip { get; set; }
        public bool IsIncluder { get; set; }
        public bool IsSubInclude { get; set; }
        public List<MemberInfo> Members { get; set; }
        public List<Expression> MemberExpressions { get; set; }
        public static ExpressionParser Create(System.Linq.Expressions.Expression expression)
        {
            ExpressionParser exp = null;
            if (expression is LambdaExpression)
            {
                var callExpression = (expression as LambdaExpression).Body as MethodCallExpression;
                if (callExpression != null)
                {
                    exp = new ExpressionParser(callExpression);
                }

                var binaryExpression = (expression as LambdaExpression).Body as BinaryExpression;
                if (binaryExpression != null)
                {
                    exp = new ExpressionParser(binaryExpression);
                }

                var unaryExpression = (expression as LambdaExpression).Body as UnaryExpression;
                if (unaryExpression != null)
                {
                    exp = new ExpressionParser(unaryExpression);
                }

                var memberExpression = (expression as LambdaExpression).Body as MemberExpression;
                if (memberExpression != null)
                {
                    exp = new ExpressionParser(memberExpression);
                }
                var newExpression = (expression as LambdaExpression).Body as NewExpression;
                if (newExpression != null)
                {
                    exp = new ExpressionParser(newExpression);
                }

                var memberInitExpression = (expression as LambdaExpression).Body as MemberInitExpression;
                if (memberInitExpression != null)
                {
                    exp = new ExpressionParser(memberInitExpression);
                }
            }
            else if (expression is BinaryExpression)
            {
                exp = new ExpressionParser(expression as BinaryExpression);
            }
            else if (expression is MethodCallExpression)
            {
                exp = new ExpressionParser(expression as MethodCallExpression);
            }
            else if (expression is UnaryExpression)
            {
                exp = new ExpressionParser(expression as UnaryExpression);
            }
            else if (expression is MemberExpression)
            {
                exp = new ExpressionParser(expression as MemberExpression);
            }
            else if (expression is Expressions.InExpression)
            {
                exp = new ExpressionParser(expression as Expressions.InExpression);
            }
            return exp;
        }
        public ExpressionParser(MemberInitExpression expression)
        {
            this.Members = new List<MemberInfo>();
            foreach (var item in expression.Bindings)
            {
                this.Members.Add(item.Member);
            }
        }
        public ExpressionParser(NewExpression expression)
        {
            this.Members = new List<MemberInfo>();
            foreach (var item in expression.Members)
            {
                this.Members.Add(item);
            }
        }
        public ExpressionParser(Expressions.InExpression expression)
        {
            this.Comparison = Comparison.Exists;
            this.Value = (expression.Expression as ConstantExpression).Value;
            this.Value2 = expression.Relation;
            this.IsQueryableDataSet = true;
            this.EntityType = this.Value.GetType().GetGenericArguments()[0];
            this.Comparison = Comparison.Exists;
        }

        public ExpressionParser(BinaryExpression expression)
        {
            if (!(expression.Left is MemberExpression))
            {
                if (expression.Left is UnaryExpression)
                {
                    if ((expression.Left as UnaryExpression).Operand is MemberExpression)
                    {
                        var memberExpression = (expression.Left as UnaryExpression).Operand as MemberExpression;
                        this.Name = memberExpression.ParsePath();
                    }
                    else
                    {
                        this.Left = ExpressionParser.Create((expression.Left as UnaryExpression).Operand);
                        this.Left.Exclude = (expression.Left as UnaryExpression).NodeType == ExpressionType.Not;
                    }
                }
                else
                {
                    this.Left = ExpressionParser.Create(expression.Left);
                    if (this.Left == null)
                        throw new Exception("Left Expression is null");
                }
            }
            else
            {
                var memberExpression = expression.Left as MemberExpression;
                this.Name = memberExpression.ParsePath();
            }
            if (expression.Right is MemberExpression && (expression.Right as MemberExpression).Expression is ConstantExpression)
            {
                var memberExpression = (expression.Right as MemberExpression);
                if (memberExpression.Member is System.Reflection.FieldInfo)
                {
                    var field = memberExpression.Member as System.Reflection.FieldInfo;
                    if (field.FieldType.IsQueryableDataSet() || field.FieldType.IsQueryable())
                    {
                        this.Value = field.FieldType;
                        this.IsQueryableDataSet = true;
                        this.EntityType = field.FieldType.GetGenericArguments()[0];
                        this.Comparison = Comparison.Exists;
                    }
                    else if (field.FieldType.IsDataEntity() || field.FieldType.IsPOCOEntity())
                    {
                        this.IsDataEntity = true;
                        this.EntityType = field.FieldType;
                    }
                    else
                    {
                        this.Value = field.GetValue((memberExpression.Expression as ConstantExpression).Value);
                    }
                }
                else if (memberExpression.Member is System.Reflection.PropertyInfo)
                {
                    var property = memberExpression.Member as System.Reflection.PropertyInfo;
                    if (property.PropertyType.IsQueryableDataSet() || property.PropertyType.IsQueryable())
                    {
                        this.Value = property.PropertyType;
                        this.IsQueryableDataSet = true;
                        this.EntityType = property.PropertyType.GetGenericArguments()[0];
                        this.Comparison = Comparison.Exists;
                    }
                    else if (property.PropertyType.IsDataEntity() || property.PropertyType.IsPOCOEntity())
                    {
                        this.IsDataEntity = true;
                        this.EntityType = property.PropertyType;
                    }
                    else
                    {
                        this.Value = property.GetValue((memberExpression.Expression as ConstantExpression).Value);
                    }
                }
            }
            else if (expression.Right is MemberExpression && (expression.Right as MemberExpression).Expression is MemberExpression && ((expression.Right as MemberExpression).Expression as MemberExpression).Expression is ConstantExpression)
            {
                this.Value = Expression.Lambda(expression.Right).Compile().DynamicInvoke();
            }
            else if (expression.Right is UnaryExpression && expression.Right.NodeType == ExpressionType.Convert)
            {
                var memberExpression = (expression.Right as UnaryExpression).Operand as MemberExpression;
                if (memberExpression != null)
                {
                    this.Value = memberExpression.GetExpressionValue(expression);
                }
                var constantExpression = (expression.Right as UnaryExpression).Operand as ConstantExpression;
                if (constantExpression != null)
                {
                    this.Value = constantExpression.Value;
                }
                var unaryExpression = (expression.Right as UnaryExpression).Operand as UnaryExpression;
                if (unaryExpression != null)
                {
                    memberExpression = unaryExpression.Operand as MemberExpression;
                    if (memberExpression != null)
                    {
                        this.Value = memberExpression.GetExpressionValue(expression);
                    }
                }
            }
            else if (!(expression.Right is ConstantExpression))
            {
                bool foundValue = false;
                if (expression.Right is MemberExpression)
                {
                    this.Value = (expression.Right as MemberExpression).GetExpressionValue(expression);
                    foundValue = true;
                }
                if (!foundValue)
                {
                    this.Right = ExpressionParser.Create(expression.Right);
                    if (this.Right == null)
                        throw new Exception("Right Expression is null");
                }
            }
            else
            {
                var consExpression = expression.Right as ConstantExpression;
                this.Value = consExpression.Value;
            }

            if (expression.NodeType == ExpressionType.And || expression.NodeType == ExpressionType.AndAlso)
            {
                this.Constraint = Constraint.And;
            }
            else if (expression.NodeType == ExpressionType.Or || expression.NodeType == ExpressionType.OrElse)
            {
                this.Constraint = Constraint.Or;
            }
            else if (expression.NodeType == ExpressionType.GreaterThan)
            {
                this.Comparison = Comparison.Greater;
            }
            else if (expression.NodeType == ExpressionType.GreaterThanOrEqual)
            {
                this.Comparison = Comparison.GreaterAndEqual;
            }
            else if (expression.NodeType == ExpressionType.LessThan)
            {
                this.Comparison = Comparison.Less;
            }
            else if (expression.NodeType == ExpressionType.LessThanOrEqual)
            {
                this.Comparison = Comparison.LessAndEqual;
            }
            else if (expression.NodeType == ExpressionType.Equal)
            {
                this.Comparison = Comparison.Equal;
            }
            else if (expression.NodeType == ExpressionType.NotEqual)
            {
                this.Comparison = Comparison.Equal;
                this.Exclude = true;
            }
            else if (expression.NodeType == ExpressionType.Not)
            {
                this.Exclude = true;
            }
        }

        public ExpressionParser(MethodCallExpression expression)
        {
            if (expression.Method.Name == "StartsWith")
                this.Comparison = Comparison.StartsWith;
            else if (expression.Method.Name == "Contains")
                this.Comparison = Comparison.Contains;
            else if (expression.Method.Name == "ContainsFTS")
                this.Comparison = Comparison.ContainsFTS;
            else if (expression.Method.Name == "EndsWith")
                this.Comparison = Comparison.EndsWith;
            else if (expression.Method.Name == "Between")
                this.Comparison = Comparison.Between;
            else if (expression.Method.Name == "Any")
                this.Comparison = Comparison.Exists;
            else if (expression.Method.Name == "Equals")
                this.Comparison = Comparison.Equal;
            else if (expression.Method.Name == "Select")
                this.IsSubInclude = true;
            else if (expression.Method.Name == "Include")
                this.IsIncluder = true;

            if (this.Comparison == Comparison.ContainsFTS)
            {
                if (expression.Arguments[0] is MemberExpression)
                    this.Value = (expression.Arguments[0] as MemberExpression).GetExpressionValue(null);
                else if (expression.Arguments[0] is ConstantExpression)
                    this.Value = (expression.Arguments[0] as ConstantExpression).Value;
                this.MemberExpressions = new List<Expression>();
                var expressions = (expression.Arguments[1] as NewArrayExpression).Expressions;
                foreach (var item in expressions)
                {
                    this.MemberExpressions.Add((item as MemberExpression));
                }
            }
            else if (expression.Object != null)
            {
                if (expression.Object is MemberExpression && expression.Object.GetType().Name.IndexOf("Field") > -1)
                {
                    // idList.Contains(op.Name)
                    this.Name = expression.Arguments[0].ParsePath();
                    this.Comparison = Comparison.In;
                    this.Value = (expression.Object as MemberExpression).GetExpressionValue(null);
                }
                else
                {
                    // op.Name.Contains("text")
                    this.Name = expression.Object.ParsePath();
                    if ((expression.Arguments[0] is ConstantExpression))
                        this.Value = (expression.Arguments[0] as ConstantExpression).Value;
                    else if ((expression.Arguments[0] is MemberExpression))
                        this.Value = (expression.Arguments[0] as MemberExpression).GetExpressionValue(null);
                }
            }
            else
            {
                var memberExpression = (expression.Arguments[0] as MemberExpression);
                if (memberExpression != null)
                {
                    this.Name = memberExpression.ParsePath();
                    if (expression.Arguments.Count > 1)
                    {
                        this.Name = expression.ParsePath();
                        if ((expression.Arguments[1] is ConstantExpression))
                        {
                            this.Value = (expression.Arguments[1] as ConstantExpression).Value;
                            if (this.Comparison == Comparison.Between)
                                this.Value2 = (expression.Arguments[2] as ConstantExpression).Value;
                        }
                        else
                        {
                            var subExpression = Create(expression.Arguments[1]);
                            this.SubExpression = subExpression;
                        }
                    }
                    if (memberExpression.Member != null)
                    {
                        var propInfo = memberExpression.Member as System.Reflection.PropertyInfo;
                        this.PropertyInfo = propInfo;
                        if (propInfo.PropertyType.IsQueryableDataSet() || propInfo.PropertyType.IsQueryable())
                        {
                            this.Value = propInfo.PropertyType;
                            this.IsQueryableDataSet = true;
                            this.EntityType = propInfo.PropertyType.GetGenericArguments()[0];
                            this.Comparison = Comparison.Exists;
                        }
                        if (propInfo.PropertyType.IsDataEntity() || propInfo.PropertyType.IsPOCOEntity())
                        {
                            this.IsDataEntity = true;
                            this.EntityType = propInfo.PropertyType;
                        }
                        if (memberExpression.Expression is MemberExpression)
                        {
                            var parentExp = memberExpression.Expression as MemberExpression;
                            if (parentExp.Member is PropertyInfo)
                                this.ParentPropertyInfo = parentExp.Member as PropertyInfo;
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
                                this.Name = unaryExp.Operand.ParsePath();
                        }
                    }
                }

                if (expression.Arguments.Count > 1)
                {
                    var consExpression = expression.Arguments[1] as ConstantExpression;
                    if (consExpression != null)
                    {
                        if (expression.Method.Name == "Take")
                            this.Take = Convert.ToInt32(consExpression.Value);
                        if (expression.Method.Name == "Skip")
                            this.Skip = Convert.ToInt32(consExpression.Value);
                    }

                    if (expression.Method.Name == "OrderBy" || expression.Method.Name == "OrderByDescending" || expression.Method.Name == "ThenBy" || expression.Method.Name == "ThenByDescending")
                    {
                        if (expression.Arguments[1] is UnaryExpression)
                        {
                            var unaryExp = expression.Arguments[1] as UnaryExpression;
                            if (unaryExp.Operand != null)
                                this.Name = unaryExp.Operand.ParsePath();
                        }
                    }
                }

                var callExpression = expression.Arguments[0] as MethodCallExpression;
                if (callExpression != null)
                {
                    var subExpression = Create(callExpression);
                    this.SubExpression = subExpression;
                }
            }
            if (expression.NodeType == ExpressionType.Not)
                this.Exclude = true;
        }

        public ExpressionParser(MemberExpression expression)
        {
            this.Name = expression.ParsePath();
            var propInfo = expression.Member as System.Reflection.PropertyInfo;
            if (propInfo != null)
            {
                this.PropertyInfo = propInfo;
                if (propInfo.PropertyType.IsQueryableDataSet() || propInfo.PropertyType.IsQueryable())
                {
                    this.Value = propInfo.PropertyType;
                    this.IsQueryableDataSet = true;
                    this.EntityType = propInfo.PropertyType.GetGenericArguments()[0];
                    this.Comparison = Comparison.Exists;
                }
                else if (propInfo.PropertyType.IsDataEntity() || propInfo.PropertyType.IsPOCOEntity())
                {
                    this.IsDataEntity = true;
                    this.EntityType = propInfo.PropertyType;
                }
                else
                {
                    if (propInfo.IsStaticProperty())
                        this.Value = propInfo.GetStaticPropertyValue();
                    else if (propInfo.PropertyType.IsAssignableFrom(typeof(bool)))
                    {
                        this.Value = true;
                    }
                }
            }
            else
            {
                var field = expression.Member as System.Reflection.FieldInfo;
                if (field.FieldType.IsQueryableDataSet() || field.FieldType.IsQueryable())
                {
                    this.Value = field.FieldType;
                    this.IsQueryableDataSet = true;
                    this.EntityType = field.FieldType.GetGenericArguments()[0];
                    this.Comparison = Comparison.Exists;
                }
                else if (field.FieldType.IsDataEntity() || field.FieldType.IsPOCOEntity())
                {
                    this.IsDataEntity = true;
                    this.EntityType = field.FieldType;
                }
                else
                {
                    this.Value = field.GetValue((expression.Expression as ConstantExpression).Value);
                }
            }
        }
        public ExpressionParser(UnaryExpression expression)
        {
            if (expression.Operand is MethodCallExpression)
            {
                var callExpression = expression.Operand as MethodCallExpression;
                this.Name = callExpression.Object.ParsePath();
                if (callExpression.Method.Name == "StartsWith")
                    this.Comparison = Comparison.StartsWith;
                else if (callExpression.Method.Name == "Contains")
                    this.Comparison = Comparison.Contains;
                else if (callExpression.Method.Name == "EndsWith")
                    this.Comparison = Comparison.EndsWith;
                else if (callExpression.Method.Name == "Any")
                    this.Comparison = Comparison.Exists;

                this.Exclude = expression.NodeType == ExpressionType.Not;

                var consExpression = callExpression.Arguments[0] as ConstantExpression;
                if (consExpression != null)
                    this.Value = consExpression.Value;

                var memberExpression = callExpression.Arguments[0] as MemberExpression;
                if (memberExpression != null)
                {
                    this.SubExpression = Create(memberExpression);
                }

                callExpression = callExpression.Arguments[0] as MethodCallExpression;
                if (callExpression != null)
                {
                    var subExpression = Create(callExpression);
                    this.SubExpression = subExpression;
                }
            }
            else if (expression.Operand is MemberExpression)
            {
                var subExpression = Create(expression.Operand as MemberExpression);
                this.SubExpression = subExpression;
                this.SubExpression.Exclude = expression.NodeType == ExpressionType.Not;
            }
            else if (expression.Operand is LambdaExpression)
            {
                var binaryExpression = ((expression.Operand as LambdaExpression).Body as BinaryExpression);
                if (binaryExpression != null)
                {
                    var subExpression = Create(binaryExpression);
                    this.SubExpression = subExpression;
                }
                else
                {
                    var memberExpression = ((expression.Operand as LambdaExpression).Body as MemberExpression);
                    if (memberExpression != null)
                    {
                        var subExpression = Create(memberExpression);
                        this.SubExpression = subExpression;
                    }
                    else
                    {
                        var subExpression = Create(expression.Operand);
                        this.SubExpression = subExpression;
                    }
                }
            }
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
            filter.Members = this.Members;
            filter.MemberExpressions = this.MemberExpressions;
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
            grouper.Members = this.Members;
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
