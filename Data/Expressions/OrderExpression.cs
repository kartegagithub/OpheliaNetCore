using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Ophelia.Data.Expressions
{
    public class OrderExpression : Expression, IDisposable
    {
        public override ExpressionType NodeType => ExpressionType.Extension;
        public override Type Type => Expression.GetType();
        public Expression Expression { get; set; }
        public bool Ascending { get; set; }
        public bool ClearBefore { get; set; }
        protected override Expression Accept(ExpressionVisitor visitor)
        {
            var specificVisitor = visitor as Querying.Query.SQLPreparationVisitor;

            return specificVisitor != null ? specificVisitor.VisitOrder(this) : base.Accept(visitor);
        }
        public OrderExpression(Expression expression, bool Ascending, bool clearBefore = false)
        {
            this.Expression = expression;
            this.Ascending = Ascending;
            this.ClearBefore = clearBefore;
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
