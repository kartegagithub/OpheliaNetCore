using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Ophelia.Data.Model
{
    internal class DataValue : IDisposable
    {
        private object _Value;
        public PropertyInfo PropertyInfo { get; set; }
        public object Value
        {
            get
            {
                return this._Value;
            }
            set
            {
                if(this._Value != value)
                    this.HasChanged = true;
                this._Value = value;
            }
        }
        public bool HasChanged { get; set; }
        public void Dispose()
        {
            this.PropertyInfo = null;
            this.Value = null;
            GC.SuppressFinalize(this);
        }
    }
}
