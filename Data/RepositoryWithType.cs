using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Data
{
    public class Repository<TEntity> : Repository where TEntity : Model.DataEntity
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

        public TEntity Create()
        {
            TEntity entity = (TEntity)Activator.CreateInstance(typeof(TEntity));
            if (entity is Model.DataEntity)
                (entity as Model.DataEntity).Tracker.State = EntityState.Loaded;
            return entity;
        }
    }
}
