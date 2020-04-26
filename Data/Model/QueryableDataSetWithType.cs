using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Ophelia.Data.Model
{
    public class QueryableDataSet<TEntity> : QueryableDataSet, IOrderedQueryable<TEntity>, ICollection<TEntity>
    {
        public QueryableDataSet(DbContext dbContext, IQueryable baseQuery, DatabaseType type) : base(dbContext, baseQuery, type)
        {

        }
        public QueryableDataSet(IQueryable<TEntity> baseQuery) : base(baseQuery)
        {

        }
        public QueryableDataSet(DataContext Context) : base(Context)
        {

        }

        public QueryableDataSet(DataContext Context, Expression expression) : base(Context, expression)
        {

        }

        protected override IList CreateList()
        {
            return new List<TEntity>();
        }

        public override bool ContainsListCollection
        {
            get
            {
                return true;
            }
        }

        public override Type ElementType
        {
            get
            {
                return typeof(TEntity);
            }
        }

        int ICollection<TEntity>.Count
        {
            get
            {
                return this.Count();
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this._list.IsReadOnly;
            }
        }

        public override void Dispose()
        {

        }
        public new List<TEntity> ToList()
        {
            this.EnsureLoad();
            return this.GetList();
        }
        public new List<TEntity> GetList()
        {
            return base.GetList() as List<TEntity>;
        }
        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
        {
            this.EnsureLoad();
            return ((List<TEntity>)this._list).GetEnumerator();
        }

        public virtual QueryableDataSet<TEntity> Include(string path)
        {
            return QueryableDataSetExtensions.Include(this, path);
        }
        public virtual QueryableDataSet<TEntity> Include<TProperty>(Expression<Func<TEntity, TProperty>> predicate)
        {
            return QueryableDataSetExtensions.Include(this, predicate);
        }
        public void Add(TEntity item)
        {
            base.Add(item);
        }

        public bool Contains(TEntity item)
        {
            return this._list.Contains(item);
        }

        public void CopyTo(TEntity[] array, int arrayIndex)
        {
            this._list.CopyTo(array, arrayIndex);
        }

        public bool Remove(TEntity item)
        {
            return base.Remove(item);
        }
    }
}
