using System;
using System.Linq;
using System.Linq.Expressions;

namespace Ophelia.Data.Expressions
{
    public class GroupExpression : Expression, IDisposable
    {
        public Expression[] Expressions { get; set; }
        public Expression Expression { get; set; }
        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => Expression.GetType();

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            var specificVisitor = visitor as Querying.Query.SQLPreparationVisitor;

            return specificVisitor != null ? specificVisitor.VisitGroup(this) : base.Accept(visitor);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Cleanup
        }

        public GroupExpression(Expression expression)
        {
            this.Expression = expression;
        }
        public GroupExpression(Expression[] expressions)
        {
            this.Expressions = expressions;
            this.Expression = expressions.FirstOrDefault();
        }
    }
}
