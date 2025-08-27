using System;
using System.Collections.Generic;
using System.Linq;

namespace Ophelia.Caching
{
    public abstract class CacheFacade<TEntity> where TEntity : class
    {
        private List<TEntity> oEntities;
        protected static object oEntity_Locker = new object();
        protected string IDColumn = "ID";
        protected abstract string Key { get; }
        protected abstract List<TEntity> GetData();
        public virtual int CacheHealthDuration { get; set; }
        public virtual int CacheDuration { get; set; }
        public virtual string KeyPrefix { get; } = "";
        protected virtual string GetKey()
        {
            return $"{this.KeyPrefix}{this.Key}";
        }
        public DateTime LastCheckDate
        {
            get
            {
                DateTime _LastCheckDate = Utility.Now;
                try
                {
                    if (CacheManager.Get(this.GetKey() + "_LCD") != null)
                    {
                        _LastCheckDate = (DateTime)CacheManager.Get(this.GetKey() + "_LCD");
                    }
                    else
                    {
                        lock (oEntity_Locker)
                        {
                            CacheManager.Add(this.GetKey() + "_LCD", _LastCheckDate);
                        }
                    }
                }
                catch (Exception)
                {
                    _LastCheckDate = Utility.Now;
                }
                return _LastCheckDate;
            }
            set
            {
                lock (oEntity_Locker)
                {
                    CacheManager.Remove(this.GetKey() + "_LCD");
                    CacheManager.Add(this.GetKey() + "_LCD", value);
                }
            }
        }
        public List<TEntity> List
        {
            get
            {
                if (!this.CheckCacheHealth())
                {
                    this.Reset();
                }
                if (this.oEntities == null)
                {
                    this.oEntities = (List<TEntity>)LocalCache.Get(this.Key);
                    if (this.oEntities == null)
                    {
                        this.oEntities = CacheManager.Get<List<TEntity>>(this.GetKey());
                        if (this.oEntities == null)
                        {
                            lock (oEntity_Locker)
                            {
                                this.oEntities = CacheManager.Get<List<TEntity>>(this.GetKey());
                                if (this.oEntities == null)
                                {
                                    this.oEntities = this.GetData();
                                    CacheManager.Add(this.GetKey(), this.oEntities, this.CacheDuration);
                                }
                            }
                        }
                        if (this.oEntities != null)
                            LocalCache.Update(this.Key, this.oEntities);
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
        public void Reset()
        {
            lock (oEntity_Locker)
            {
                this.oEntities = null;
            }
        }
        public void DropCache()
        {
            this.Reset();
            lock (oEntity_Locker)
            {
                CacheManager.Remove(this.GetKey());
                LocalCache.Remove(this.Key);
            }
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
            lock (oEntity_Locker)
            {
                TEntity entity = this.List.Where(this.IDColumn, id).FirstOrDefault();
                if (entity != null)
                {
                    this.List.Remove(entity);
                }
            }
        }

        public void Update(TEntity entity)
        {
            this.Remove(entity.GetPropertyValue(this.IDColumn));
            lock (oEntity_Locker)
            {
                if (this.CanAdd(entity))
                    this.List.Add(entity);
                this.SetCacheDirty();
            }
        }

        protected virtual bool CheckCacheHealth()
        {
            if (Utility.Now.Subtract(this.LastCheckDate).TotalMinutes > this.CacheHealthDuration)
            {
                var Result = this.CheckPersistentCacheHealth();
                this.LastCheckDate = Utility.Now;
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
            this.CacheDuration = 1440;
        }
    }
}
