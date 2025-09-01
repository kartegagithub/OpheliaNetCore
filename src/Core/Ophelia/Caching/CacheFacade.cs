using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ophelia.Caching
{
    public abstract class CacheFacade<TEntity> where TEntity : class
    {
        private List<TEntity> oEntities;
        protected static object oEntity_Locker = new object();

        /// <summary>
        /// The name of the property to be used as the unique identifier for the entity. Default is "ID".
        /// </summary>
        protected string IDColumn { get; set; } = "ID";

        /// <summary>
        /// This property should be overridden to provide a unique key for the cache.
        /// </summary>
        protected abstract string Key { get; }

        /// <summary>
        /// This method should be overridden to provide the data to be cached.
        /// </summary>
        /// <returns></returns>
        protected abstract List<TEntity> GetData();

        /// <summary>
        /// If true, it will use local cache to store the data in memory for faster access. Default is true. If you intend to use inmemory caching, set it to false. Otherwise, it may lead to high memory consumption.
        /// </summary>
        protected bool UseLocalCache { get; set; } = true;

        /// <summary>
        /// Duration in minutes to check the health of the cache. If the cache is older than this duration, it will be reloaded. Default is 10 minutes.
        /// </summary>
        public virtual int CacheAutoReloadDuration { get; set; }

        /// <summary>
        /// Duration in minutes to keep the data in cache. Default is 1440 minutes (1 day).
        /// </summary>
        public virtual int CacheDuration { get; set; }

        /// <summary>
        /// A prefix to be added to the cache key. Default is an empty string.
        /// </summary>
        public virtual string KeyPrefix { get; } = "";
        protected virtual string GetKey()
        {
            return $"{this.KeyPrefix}{this.Key}";
        }

        /// <summary>
        /// The last time the cache health was checked.
        /// </summary>
        public DateTime LastCheckDate
        {
            get
            {
                DateTime _LastCheckDate = Utility.Now;
                var key = this.GetKey() + "_LCD";
                try
                {
                    var objDate = LocalCache.Get(key);
                    if (objDate != null)
                    {
                        _LastCheckDate = (DateTime)objDate;
                    }
                    else
                    {
                        lock (oEntity_Locker)
                        {
                            LocalCache.Remove(key);
                            LocalCache.Update(key, _LastCheckDate);
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
                    var key = this.GetKey() + "_LCD";
                    LocalCache.Remove(key);
                    LocalCache.Update(key, value);
                }
            }
        }

        /// <summary>
        /// The list of entities cached.
        /// </summary>
        public List<TEntity> List
        {
            get
            {
                try
                {
                    if (!this.CheckCacheHealth())
                        this.Reset();

                    if (this.oEntities == null)
                    {
                        var key = this.GetKey();
                        if (this.UseLocalCache)
                            this.oEntities = (List<TEntity>)LocalCache.Get(key);
                        if (this.oEntities == null)
                        {
                            this.oEntities = CacheManager.Get<List<TEntity>>(key);
                            if (this.oEntities == null)
                            {
                                lock (oEntity_Locker)
                                {
                                    this.oEntities = CacheManager.Get<List<TEntity>>(key);
                                    if (this.oEntities == null)
                                    {
                                        this.oEntities = this.GetData();
                                        CacheManager.Add(key, this.oEntities, this.CacheDuration);
                                    }
                                }
                            }
                            if (this.oEntities != null && this.UseLocalCache)
                                LocalCache.Update(key, this.oEntities);
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.OnFail(ex);
                }
                return this.oEntities;
            }
        }

        protected void OnFail(Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        /// <summary>
        /// Gets an entity by its unique identifier.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TEntity Get(object id)
        {
            if (id != null)
            {
                return this.Get(this.IDColumn, id);
            }
            return null;
        }

        /// <summary>
        /// Gets an entity by a specified property and its value.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Finds entities that match a specified predicate.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public List<TEntity> Find(Func<TEntity, bool> predicate)
        {
            return this.List.Where(predicate).ToList();
        }

        /// <summary>
        /// Resets the cached entities, forcing a reload on the next access.
        /// </summary>
        public void Reset()
        {
            lock (oEntity_Locker)
            {
                var key = this.GetKey();
                this.oEntities = null;
                if (this.UseLocalCache)
                    LocalCache.Remove(key);
            }
        }

        /// <summary>
        /// Drops the cache for this entity type.
        /// </summary>
        public void DropCache()
        {
            this.Reset();
            lock (oEntity_Locker)
            {
                var key = this.GetKey();
                CacheManager.Remove(key);
            }
        }

        /// <summary>
        /// Reloads the cached entities, optionally marking the cache as dirty.
        /// </summary>
        /// <param name="CanSetCacheDirty"></param>
        /// <returns></returns>
        public bool Reload(bool CanSetCacheDirty = true)
        {
            this.DropCache();
            if (CanSetCacheDirty)
                this.SetCacheDirty();
            return this.Load();
        }

        /// <summary>
        /// Loads the cached entities if available.
        /// </summary>
        /// <returns></returns>
        public bool Load()
        {
            return this.List.Count > 0;
        }

        /// <summary>
        /// Removes an entity from the cache by its unique identifier.
        /// </summary>
        /// <param name="id"></param>
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

        /// <summary>
        /// Adds a new entity to the cache.
        /// </summary>
        /// <param name="entity"></param>
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

        /// <summary>
        /// Checks the health of the cache and reloads it if necessary.
        /// </summary>
        /// <returns></returns>
        protected virtual bool CheckCacheHealth()
        {
            if (this.CacheAutoReloadDuration > 0 && Utility.Now.Subtract(this.LastCheckDate).TotalMinutes > this.CacheAutoReloadDuration)
            {
                this.LastCheckDate = Utility.Now;
                return false;
            }
            return true;
        }

        /// <summary>
        /// This method can be overridden to mark the cache as dirty, prompting a reload.
        /// </summary>
        protected virtual void SetCacheDirty()
        {

        }

        /// <summary>
        /// This method can be overridden to determine if an entity can be added to the cache.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected virtual bool CanAdd(TEntity entity)
        {
            return true;
        }

        public CacheFacade()
        {
            this.CacheAutoReloadDuration = 10;
            this.CacheDuration = 1440;
        }
    }
}
