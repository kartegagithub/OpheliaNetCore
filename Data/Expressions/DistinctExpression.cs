using System;
using System.Linq.Expressions;

namespace Ophelia.Data.Expressions
{
    public class DistinctExpression : SelectExpression
    {
        protected override Expression Accept(ExpressionVisitor visitor)
        {
            var specificVisitor = visitor as Querying.Query.SQLPreparationVisitor;

            return specificVisitor != null ? specificVisitor.VisitDistinct(this) : base.Accept(visitor);
        }

        public DistinctExpression(Expression expression) : base(expression)
        {

        }
    }
}