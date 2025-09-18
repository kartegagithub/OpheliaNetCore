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
                                        this.LoadData();
                                    }
                                }
                            }
                            if (this.oEntities != null && this.UseLocalCache)
                            {
                                LocalCache.Update(key, this.oEntities);
                                this.OnStateChange("LOCALY_CACHED");
                            }
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

        /// <summary>
        /// Loads and caches data
        /// </summary>
        protected void LoadData()
        {
            this.OnStateChange("LOADING");
            this.oEntities = this.GetData();
            this.OnStateChange("LOADED");
            this.Commit();
        }

        /// <summary>
        /// Updates cache
        /// </summary>
        public void Commit()
        {
            CacheManager.Add(this.GetKey(), this.oEntities, this.CacheDuration);
        }

        /// <summary>
        /// Logs exception
        /// </summary>
        /// <param name="ex"></param>
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
            TEntity data = null;
            if (id != null)
            {
                data = this.Get(this.IDColumn, id);
            }
            return data;
        }

        /// <summary>
        /// Return true to reload cache if persistent data exists but cache does not, otherwise false. 
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected bool OnCacheNotFound(string property, object value)
        {
            return false;
        }

        /// <summary>
        /// Return true to reload cache if persistent data exists but cache does not, otherwise false. 
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        protected bool OnCacheNotFound(Func<TEntity, bool> predicate)
        {
            return false;
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
            if (this.OnCacheNotFound(property, value))
            {
                this.Reload();
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
            var data = this.List.Where(predicate).ToList();
            if (data.Count == 0)
            {
                if (this.OnCacheNotFound(predicate))
                {
                    this.Reload();
                }
            }
            return data;
        }

        /// <summary>
        /// Resets the cached entities, forcing a reload on the next access.
        /// </summary>
        public void Reset()
        {
            this.OnStateChange("RESET");
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
                this.OnStateChange("DROPPED");
                var key = this.GetKey();
                CacheManager.Remove(key);
            }
        }

        /// <summary>
        /// Drops and reloads the cached entities.
        /// </summary>
        /// <returns></returns>
        public bool Reload()
        {
            try
            {
                this.DropCache();
                this.LoadData();
                return true;
            }
            catch (Exception ex)
            {
                this.OnFail(ex);
                return false;
            }
        }

        /// <summary>
        /// Removes an entity from the cache by its unique identifier.
        /// </summary>
        /// <param name="id"></param>
        public void Remove(object id, bool commit = true)
        {
            lock (oEntity_Locker)
            {
                TEntity entity = this.List.Where(this.IDColumn, id).FirstOrDefault();
                if (entity != null)
                {
                    this.List.Remove(entity);
                    if (commit)
                        this.Commit();
                }
            }
        }

        /// <summary>
        /// Adds a new entity to the cache.
        /// </summary>
        /// <param name="entity"></param>
        public void Update(TEntity entity, bool commit = true)
        {
            this.Remove(entity.GetPropertyValue(this.IDColumn));
            lock (oEntity_Locker)
            {
                if (this.CanAdd(entity))
                {
                    this.List.Add(entity);
                    if (commit)
                        this.Commit();
                }
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
                this.OnStateChange("INVALIDATED");
                this.LastCheckDate = Utility.Now;
                return false;
            }
            return true;
        }
        protected void OnStateChange(string state)
        {
            Console.WriteLine($"Cache_State:{this.GetKey()}::{state}");
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
