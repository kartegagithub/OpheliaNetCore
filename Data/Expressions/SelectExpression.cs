using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Ophelia.Data.Expressions
{
    public class SelectExpression : Expression, IDisposable
    {
        public Expression Expression { get; set; }
        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => Expression.GetType();

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            var specificVisitor = visitor as Querying.Query.SQLPreparationVisitor;

            return specificVisitor != null ? specificVisitor.VisitSelect(this) : base.Accept(visitor);
        }

        public SelectExpression(Expression expression)
        {
            this.Expression = expression;
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
