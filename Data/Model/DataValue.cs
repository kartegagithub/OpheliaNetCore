﻿using System;
using System.Reflection;

namespace Ophelia.Data.Model
{
    [Serializable]
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
                if (this._Value != value)
                    this.HasChanged = true;
                this._Value = value;
            }
        }
        public bool HasChanged { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.PropertyInfo = null;
            this.Value = null;
        }
    }
}
