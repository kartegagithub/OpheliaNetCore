using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;
using Ophelia.Reflection;

namespace Ophelia.Web.Application.Server
{
    public abstract class CacheFacade<TEntity> where TEntity : class
    {
        private List<TEntity> oEntities;
        protected static object oEntity_Locker = new object();
        protected string IDColumn = "ID";
        protected abstract string Key { get; }
        protected abstract List<TEntity> GetData();
        public virtual int CacheHealthDuration { get; set; }
        public DateTime LastCheckDate
        {
            get
            {
                DateTime LastCheckDate = DateTime.Now;
                try
                {
                    if (CacheManager.Get(this.Key + "_LCD") != null)
                    {
                        LastCheckDate = (DateTime)CacheManager.Get(this.Key + "_LCD");
                    }
                    else
                    {
                        CacheManager.Add(this.Key + "_LCD", LastCheckDate);
                    }
                }
                catch (Exception)
                {

                }
                return LastCheckDate;
            }
            set
            {
                try
                {
                    CacheManager.Remove(this.Key + "_LCD");
                    CacheManager.Add(this.Key + "_LCD", value);
                }
                catch (Exception)
                {

                }
            }
        }
        public List<TEntity> List
        {
            get
            {
                if (!this.CheckCacheHealth())
                {
                    this.DropCache();
                }
                this.oEntities = (List<TEntity>)CacheManager.Get(this.Key);
                if (this.oEntities == null)
                {
                    lock (oEntity_Locker)
                    {
                        this.oEntities = (List<TEntity>)CacheManager.Get(this.Key);
                        if (this.oEntities == null)
                        {
                            this.oEntities = this.GetData();
                            CacheManager.Add(this.Key, this.oEntities);
                        }
                    }
                }
                return this.oEntities;
            }
        }

        public TEntity Get(object id)
        {
            if (id != null)
            {
                return this.Get(this.IDColumn, id);
            }
            return null;
        }

        public TEntity Get(string property, object value)
        {
            var convertedData = typeof(TEntity).GetProperty(property).PropertyType.ConvertData(value);
            foreach (var item in this.List)
            {
                var val = item.GetPropertyValue(property);
                if (val.Equals(convertedData))
                {
                    return item;
                }
            }
            return null;
        }

        public List<TEntity> Find(Func<TEntity, bool> predicate)
        {
            return this.List.Where(predicate).ToList();
        }

        public void DropCache()
        {
            CacheManager.Remove(this.Key);
        }

        public bool Reload(bool CanSetCacheDirty = true)
        {
            this.DropCache();
            if (CanSetCacheDirty)
                this.SetCacheDirty();
            return this.Load();
        }

        public bool Load()
        {
            return this.List.Count > 0;
        }

        public void Remove(object id)
        {
            TEntity entity = this.List.Where(this.IDColumn, id).FirstOrDefault();
            if (entity != null)
            {
                this.List.Remove(entity);
            }
        }

        public void Update(TEntity entity)
        {
            this.Remove(entity.GetPropertyValue(this.IDColumn));
            if (this.CanAdd(entity))
                this.List.Add(entity);
            this.SetCacheDirty();
        }

        protected bool CheckCacheHealth()
        {

            if (DateTime.Now.Subtract(this.LastCheckDate).TotalMinutes > this.CacheHealthDuration)
            {
                var Result = this.CheckPersistentCacheHealth();
                this.LastCheckDate = DateTime.Now;
                return Result;
            }
            return true;
        }

        protected virtual bool CheckPersistentCacheHealth()
        {
            return true;
        }

        protected virtual void SetCacheDirty()
        {

        }

        protected virtual bool CanAdd(TEntity entity)
        {
            return true;
        }

        public CacheFacade()
        {
            this.CacheHealthDuration = 10;
        }
    }
}
