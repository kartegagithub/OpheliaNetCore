using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Ophelia.Data.Expressions
{
    public class ExcludeExpression : Expression, IDisposable
    {
        public Expression Expression { get; set; }
        public string Path { get; set; }
        public bool ExcludeFromAllQueries { get; set; }
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

        public ExcludeExpression(Expression exp, bool excludeFromAllQueries)
        {
            this.Expression = exp;
            this.ExcludeFromAllQueries = excludeFromAllQueries;
        }

        public ExcludeExpression(string path, bool excludeFromAllQueries)
        {
            this.Path = path;
            this.ExcludeFromAllQueries = excludeFromAllQueries;
        }

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            var specificVisitor = visitor as Querying.Query.SQLPreparationVisitor;

            return specificVisitor != null? specificVisitor.VisitExclude(this): base.Accept(visitor);
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
