using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Reflection;

namespace Ophelia.Data.Querying.Query
{
    public class SQLPreparationVisitor : ExpressionVisitor, IDisposable
    {
        private QueryData _Data;
        public QueryData Data
        {
            get
            {
                return this._Data;
            }
        }

        public SQLPreparationVisitor(QueryData data)
        {
            this._Data = data;
        }

        #region "Overloaded"
        public override Expression Visit(Expression node)
        {
            return base.Visit(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == "Take")
            {
                if (node.Arguments[1] is ConstantExpression)
                    this.Data.PageSize = Convert.ToInt32((node.Arguments[1] as ConstantExpression).Value);
                else if (node.Arguments[1] is Expressions.TakeExpression)
                    this.Data.PageSize = (node.Arguments[1] as Expressions.TakeExpression).Count;
            }
            else if (node.Method.Name == "Skip")
            {
                if (node.Arguments[1] is ConstantExpression)
                    this.Data.SkippedCount = Convert.ToInt32((node.Arguments[1] as ConstantExpression).Value);
                else if (node.Arguments[1] is Expressions.SkipExpression)
                    this.Data.SkippedCount = (node.Arguments[1] as Expressions.SkipExpression).Count;
            }
            else if (node.Method.Name == "OrderBy" || node.Method.Name == "ThenBy")
                this.Data.Sorters.Add(Helpers.Sorter.Create(node, true));
            else if (node.Method.Name == "OrderByDescending" || node.Method.Name == "ThenByDescending")
                this.Data.Sorters.Add(Helpers.Sorter.Create(node, false));
            else if (node.Method.Name == "Sum" || node.Method.Name == "Avg" || node.Method.Name == "Max" || node.Method.Name == "Min")
            {
                var isAggregiate = true;
                this.Data.Functions.Add(Helpers.DBFunction.Create(node.Method.Name, isAggregiate, node.Arguments[1]));
            }
            else if (node.Method.Name == "GroupBy")
            {
                if (node.Arguments.Count > 1 && !(node.Arguments[1] is Expressions.GroupExpression))
                    this.Data.Groupers.Add(Helpers.Grouper.Create(node.Arguments[1]));
            }
            else if (node.Method.Name == "Select")
            {
                if (node.Arguments.Count > 1 && !(node.Arguments[1] is Expressions.SelectExpression))
                    this.Data.Selectors.Add(Helpers.Selector.Create(node.Arguments[1]));
            }
            else if (node.Method.Name == "Include")
            {
                //if (node.Arguments.Count > 1)
                //{
                //    var arg = node.Arguments[1];
                //    if (arg is Expressions.IncludeExpression)
                //        this.VisitInclude(arg as Expressions.IncludeExpression);
                //}
            }
            else if (node.Method.Name == "Where")
            {
                Expression arg = null;
                if (node.Arguments.Count > 1)
                    arg = node.Arguments[1];
                if (arg != null)
                {
                    if (!(arg is Expressions.WhereExpression))
                    {
                        if (this.Data.Filter == null)
                            this.Data.Filter = Helpers.Filter.Create(arg);
                        else
                            this.Data.Filter = new Helpers.Filter()
                            {
                                Left = Helpers.Filter.Create(arg),
                                Right = this.Data.Filter,
                                Constraint = Constraint.And
                            };
                    }
                }
            }
            return base.VisitMethodCall(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            return base.VisitConstant(node);
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            return base.VisitConditional(node);
        }

        protected override Expression VisitExtension(Expression node)
        {
            return base.VisitExtension(node);
        }

        protected override Expression VisitDynamic(System.Linq.Expressions.DynamicExpression node)
        {
            return base.VisitDynamic(node);
        }

        protected override Expression VisitLoop(LoopExpression node)
        {
            return base.VisitLoop(node);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            return base.VisitLambda<T>(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            return base.VisitMember(node);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            return base.VisitUnary(node);
        }

        #endregion

        public virtual Expression VisitInclude(Expressions.IncludeExpression expression)
        {
            Helpers.Includer includer = null;
            if (expression.Expression != null)
                includer = Helpers.Includer.Create(expression.Expression, expression.JoinType);
            else
                includer = Helpers.Includer.Create(this.Data.EntityType.GetPropertyInfo(expression.Path), expression.Path, expression.JoinType);

            //TODO: Burada derin include (3 ve üzeri) eksik. Include(op => op.Customer.BusinessPartner.BusinessPartnerType)
            //var parent = this.GetParentIncluder(includer.Name);

            if (includer.Name.IndexOf(".") > -1)
            {
                Helpers.Includer parent = null;
                var baseIncluders = includer.Name.Split('.');
                for (int i = 0; i < baseIncluders.Length; i++)
                {
                    var propName = "";
                    for (int j = 0; j <= i; j++)
                    {
                        if (!string.IsNullOrEmpty(propName))
                            propName += ".";
                        propName += baseIncluders[j];
                    }
                    var tmpIncluder = Helpers.Includer.Create(this.Data.EntityType.GetPropertyInfo(propName), propName, expression.JoinType);
                    parent = tmpIncluder;
                    if (propName.IndexOf(".") > -1)
                    {
                        var tmpParent = this.Data.Includers.GetIncluder(tmpIncluder.Name);
                        if (tmpParent.Name != tmpIncluder.Name && !tmpParent.SubIncluders.Where(op => op.Name == tmpIncluder.Name).Any())
                            tmpParent.SubIncluders.Add(tmpIncluder);
                    }
                    else
                    {
                        if (!this.Data.Includers.Where(op => op.Name == tmpIncluder.Name).Any())
                            this.Data.Includers.Add(tmpIncluder);
                        else
                            parent = this.Data.Includers.Where(op => op.Name == tmpIncluder.Name).FirstOrDefault();
                    }
                }
                //if (includer.SubIncluders.Count == 0)
                //{
                //    if (!parent.SubIncluders.Where(op => op.Name == includer.Name).Any())
                //        parent.SubIncluders.Add(includer);
                //}
                //else
                //{
                //    parent.SubIncluders.AddRange(includer.SubIncluders);
                //}
            }
            else if (!this.Data.Includers.Where(op => op.Name == includer.Name).Any())
            {
                this.Data.Includers.Add(includer);
            }
            return expression;
        }
        //private Helpers.Includer GetParentIncluder(string path, Helpers.Includer parent = null)
        //{
        //    if (path.IndexOf(".") > -1)
        //    {
        //        var baseIncluders = path.Split('.');
        //        for (int i = 0; i < baseIncluders.Length - 1; i++)
        //        {
        //            var propName = "";
        //            for (int j = 0; j <= i; j++)
        //            {
        //                if (!string.IsNullOrEmpty(propName))
        //                    propName += ".";
        //                propName += baseIncluders[j];
        //            }
        //            if (parent == null && i == 0) {
        //                parent = this.Data.Includers.Where(op => op.Name == propName).FirstOrDefault();
        //                if (parent == null)
        //                    return null;
        //                else {
        //                    return this.GetParentIncluder(propName, parent);
        //                }
        //            }
        //            else if(parent != null)
        //            {
        //                if (parent.SubIncluders.Where(op => op.Name == propName).Any()) {

        //                }
        //            }
        //        }
        //    }
        //    return this.Data.Includers.Where(op => op.Name == path).FirstOrDefault();
        //}
        public virtual Expression VisitExclude(Expressions.ExcludeExpression expression)
        {
            this.Data.Excluders.Add(Helpers.Excluder.Create(expression.Expression, expression.ExcludeFromAllQueries));
            return expression;
        }
        public virtual Expression VisitOrder(Expressions.OrderExpression expression)
        {
            if (expression.ClearBefore)
                this.Data.Sorters.Clear();

            var sorter = Helpers.Sorter.Create(expression.Expression, expression.Ascending);
            if (this.Data.Sorters.Where(op => op.Name == sorter.Name).Any())
            {
                var tmp = this.Data.Sorters.Where(op => op.Name == sorter.Name).FirstOrDefault();
                this.Data.Sorters.Remove(tmp);
            }
            this.Data.Sorters.Add(sorter);
            return expression;
        }

        public virtual Expression VisitWhere(Expressions.WhereExpression expression)
        {
            if (this.Data.Filter == null)
                this.Data.Filter = Helpers.Filter.Create(expression.Expression);
            else
                this.Data.Filter = new Helpers.Filter()
                {
                    Left = Helpers.Filter.Create(expression.Expression),
                    Right = this.Data.Filter,
                    Constraint = Constraint.And
                };
            return expression;
        }

        public virtual Expression VisitIn(Expressions.InExpression expression)
        {
            if (this.Data.Filter == null)
                this.Data.Filter = Helpers.Filter.Create(expression);
            else
                this.Data.Filter = new Helpers.Filter()
                {
                    Left = Helpers.Filter.Create(expression),
                    Right = this.Data.Filter,
                    Constraint = Constraint.And
                };
            return expression;
        }

        public virtual Expression VisitTake(Expressions.TakeExpression expression)
        {
            this.Data.PageSize = expression.Count;
            return expression;
        }

        public virtual Expression VisitSkip(Expressions.SkipExpression expression)
        {
            this.Data.SkippedCount = expression.Count;
            return expression;
        }
        public virtual Expression VisitUpdater(Expressions.UpdateExpression expression)
        {
            return expression;
        }

        public virtual Expression VisitGroup(Expressions.GroupExpression expression)
        {
            this.Data.Groupers.Add(Helpers.Grouper.Create(expression.Expression));
            return expression;
        }

        public virtual Expression VisitSelect(Expressions.SelectExpression expression)
        {
            this.Data.Selectors.Add(Helpers.Selector.Create(expression.Expression));
            return expression;
        }

        private void AddWhereClause(Expression expression)
        {
            var callExpression = (expression as LambdaExpression).Body as MethodCallExpression;
            if (callExpression != null)
            {
                this.AddWhereClause(callExpression);
            }

            var binaryExpression = (expression as LambdaExpression).Body as BinaryExpression;
            if (binaryExpression != null)
            {
                this.AddWhereClause(binaryExpression);
            }
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}