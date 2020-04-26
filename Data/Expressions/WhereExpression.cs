using System;
using System.Linq.Expressions;

namespace Ophelia.Data.Expressions
{
    public class WhereExpression : Expression, IDisposable
    {
        public Expression Expression { get; set; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => Expression.GetType();

        public Attributes.N2NRelationProperty Relation { get; set; }

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            var specificVisitor = visitor as Querying.Query.SQLPreparationVisitor;

            return specificVisitor != null ? specificVisitor.VisitWhere(this) : base.Accept(visitor);
        }

        public WhereExpression(Expression expression)
        {
            this.Expression = expression;
        }

        public WhereExpression(Expression expression, Attributes.N2NRelationProperty relation)
        {
            this.Expression = expression;
            this.Relation = relation;
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
    }
}
