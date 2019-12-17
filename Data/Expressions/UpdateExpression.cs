using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Ophelia.Data.Expressions
{
    public class UpdateExpression : Expression, IDisposable
    {
        public Expression Expression { get; set; }
        public object Value { get; set; }
        public override ExpressionType NodeType => ExpressionType.Extension;
        public override Type Type => Expression.GetType();

        public UpdateExpression(Expression exp, object value)
        {
            this.Expression = exp;
            this.Value = value;
        }

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            var specificVisitor = visitor as Querying.Query.SQLPreparationVisitor;

            return specificVisitor != null? specificVisitor.VisitUpdater(this): base.Accept(visitor);
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
