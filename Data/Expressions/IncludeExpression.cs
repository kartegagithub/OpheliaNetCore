using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Ophelia.Data.Expressions
{
    public class IncludeExpression: Expression, IDisposable
    {
        public Expression Expression { get; set; }
        public string Path { get; set; }
        public JoinType JoinType { get; set; }

        public override ExpressionType NodeType => ExpressionType.Extension;
        public override Type Type
        {
            get
            {
                if (this.Expression != null)
                    return Expression.GetType();
                else
                    return typeof(String);
            }
        }

        public IncludeExpression(Expression exp, JoinType joinType)
        {
            this.Expression = exp;
            this.JoinType = joinType;
        }

        public IncludeExpression(string path, JoinType joinType)
        {
            this.Path = path;
            this.JoinType = joinType;
        }

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            var specificVisitor = visitor as Querying.Query.SQLPreparationVisitor;

            return specificVisitor != null? specificVisitor.VisitInclude(this): base.Accept(visitor);
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
