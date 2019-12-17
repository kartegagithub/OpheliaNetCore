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
    public abstract class CacheFacade
    {
        private object oData;
        protected static object oEntity_Locker = new object();
        protected abstract string Key { get; }
        protected abstract object GetData();

        public object Data
        {
            get
            {
                this.oData = CacheManager.Get(this.Key);
                if (this.oData == null)
                {
                    lock (oEntity_Locker)
                    {
                        this.oData = CacheManager.Get(this.Key);
                        if (this.oData == null)
                        {
                            this.oData = this.GetData();
                            CacheManager.Add(this.Key, this.oData);
                        }
                    }
                }
                return this.oData;
            }
        }

        public void DropCache()
        {
            CacheManager.Remove(this.Key);
        }

        public bool Load() {
            return this.Data != null;
        }

        public void Update(object Data)
        {
            this.DropCache();
            CacheManager.Add(this.Key, Data);
        }
    }
}
