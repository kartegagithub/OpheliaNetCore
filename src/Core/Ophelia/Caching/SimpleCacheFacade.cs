using AngleSharp.Dom;
using System.Collections.Generic;

namespace Ophelia.Caching
{
    public abstract class CacheFacade
    {
        private object oData;
        protected static object oEntity_Locker = new object();
        protected abstract string Key { get; }
        protected abstract object GetData();
        public virtual string KeyPrefix { get; } = "";
        protected bool UseLocalCache { get; set; } = true;
        protected virtual string GetKey()
        {
            return $"{this.KeyPrefix}{this.Key}";
        }
        public object Data
        {
            get
            {
                var key = this.GetKey();
                if (this.UseLocalCache)
                    this.oData = LocalCache.Get(key);

                if (this.oData == null)
                {
                    this.oData = CacheManager.Get(key);
                    if (this.oData == null)
                    {
                        lock (oEntity_Locker)
                        {
                            this.oData = CacheManager.Get(key);
                            if (this.oData == null)
                            {
                                this.oData = this.GetData();
                                CacheManager.Add(key, this.oData);
                            }
                        }
                    }
                    if (this.oData != null && this.UseLocalCache)
                        LocalCache.Update(key, this.oData);
                }
                    
                return this.oData;
            }
        }

        public void DropCache()
        {
            this.Reset();
            lock (oEntity_Locker)
            {
                var key = this.GetKey();
                CacheManager.Remove(key);
                if (this.UseLocalCache)
                    LocalCache.Remove(key);
            }
        }

        public bool Reload(bool CanSetCacheDirty = true)
        {
            this.DropCache();
            return this.Load();
        }
        public void Reset()
        {
            lock (oEntity_Locker)
            {
                this.oData = null;
            }
        }
        public bool Load()
        {
            return this.Data != null;
        }

        public void Update(object Data)
        {
            this.DropCache();
            var key = this.GetKey();
            CacheManager.Add(key, Data);
            if (this.UseLocalCache)
                LocalCache.Update(key, Data);
        }
    }
}
