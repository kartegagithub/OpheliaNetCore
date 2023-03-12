using System;

namespace Ophelia.Data
{
    public class Repository<TEntity> : Repository
    {
        public Model.QueryableDataSet<TEntity> GetQuery()
        {
            return new Model.QueryableDataSet<TEntity>(this.Context);
        }
        public object TruncateData()
        {
            return this.Context.Connection.ExecuteNonQuery("TRUNCATE TABLE " + this.Context.Connection.GetTableName(typeof(TEntity)));
        }
        public bool SaveChanges(TEntity entity)
        {
            return base.SaveChanges(entity);
        }
        public Repository(DataContext Context) : base(Context)
        {

        }
        public TEntity Track(TEntity entity)
        {
            return (TEntity)base.Track(entity);
        }
        public TEntity Create()
        {
            TEntity entity = (TEntity)Activator.CreateInstance(typeof(TEntity));
            if (entity is Model.DataEntity)
                (entity as Model.DataEntity).Tracker.State = EntityState.Loaded;
            else
                entity = this.Track(entity);
            return entity;
        }
    }
}
