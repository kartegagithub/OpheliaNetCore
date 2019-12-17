using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Ophelia.Data.Expressions
{
    public class InExpression : Expression, IDisposable
    {
        public Expression Expression { get; set; }

        public override ExpressionType NodeType => ExpressionType.Constant;

        public override Type Type => this.GetType();

        public Attributes.N2NRelationProperty Relation { get; set; }

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            var specificVisitor = visitor as Querying.Query.SQLPreparationVisitor;

            return specificVisitor != null ? specificVisitor.VisitIn(this) : base.Accept(visitor);
        }

        public InExpression(Expression expression)
        {
            this.Expression = expression;
        }

        public InExpression(Expression expression, Attributes.N2NRelationProperty relation)
        {
            this.Expression = expression;
            this.Relation = relation;
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
