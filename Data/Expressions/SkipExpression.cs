﻿using System;
using System.Linq.Expressions;

namespace Ophelia.Data.Expressions
{
    public class SkipExpression : Expression, IDisposable
    {
        public int Count { get; set; }
        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => typeof(int);

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            var specificVisitor = visitor as Querying.Query.SQLPreparationVisitor;

            return specificVisitor != null ? specificVisitor.VisitSkip(this) : base.Accept(visitor);
        }

        public SkipExpression(int count)
        {
            this.Count = count;
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
