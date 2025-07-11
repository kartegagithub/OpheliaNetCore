﻿namespace Ophelia.Caching
{
    public abstract class CacheFacade
    {
        private object oData;
        protected static object oEntity_Locker = new object();
        protected abstract string Key { get; }
        protected abstract object GetData();
        public virtual string KeyPrefix { get; } = "";
        protected virtual string GetKey()
        {
            return $"{this.KeyPrefix}{this.Key}";
        }
        public object Data
        {
            get
            {
                this.oData = CacheManager.Get(this.GetKey());
                if (this.oData == null)
                {
                    lock (oEntity_Locker)
                    {
                        this.oData = CacheManager.Get(this.GetKey());
                        if (this.oData == null)
                        {
                            this.oData = this.GetData();
                            CacheManager.Add(this.GetKey(), this.oData);
                        }
                    }
                }
                return this.oData;
            }
        }

        public void DropCache()
        {
            CacheManager.Remove(this.GetKey());
        }

        public bool Load()
        {
            return this.Data != null;
        }

        public void Update(object Data)
        {
            this.DropCache();
            CacheManager.Add(this.GetKey(), Data);
        }
    }
}
