using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Data.Model
{
    public class PocoEntityTracker
    {
        public object Entity { get; set; }
        public object ProxyEntity { get; set; }
        public Type EntityType { get; set; }
        public Dictionary<PropertyInfo, object> OriginalValues { get; set; }
        public List<DataValue> Changes { get; set; }
        private bool? _HasChanged;
        public bool HasChanged
        {
            get
            {
                if (!_HasChanged.HasValue)
                    this.GetChanges();
                return _HasChanged.GetValueOrDefault(false);
            }
            set { _HasChanged = value; }
        }
        public virtual bool IsNewRecord()
        {
            var pks = Extensions.GetPrimaryKeyProperties(this.EntityType);
            foreach (var pk in pks)
            {
                if (pk.PropertyType.GetDefaultValue().Equals(pk.GetValue(this.Entity)))
                    return true;
            }
            return false;
        }
        public virtual List<DataValue> GetChanges()
        {
            var list = new List<DataValue>();
            foreach (var item in this.OriginalValues)
            {
                var value = item.Key.GetValue(this.ProxyEntity);
                if ((value != null && item.Value == null) || (value == null && item.Value != null) || (!value.Equals(item.Value)))
                    list.Add(new DataValue() { HasChanged = true, PropertyInfo = item.Key, Value = value });
            }
            this.Changes = list;
            this._HasChanged = list.Any();
            return list;
        }
        internal virtual void OnAfterCreateEntity()
        {

        }
        internal virtual void OnBeforeUpdateEntity()
        {

        }
        internal virtual void OnBeforeInsertEntity()
        {

        }
        internal virtual void OnAfterUpdateEntity()
        {

        }

        internal virtual void OnAfterDeleteEntity()
        {

        }
        public PocoEntityTracker(object entity, object proxyEntity)
        {
            Entity = entity;
            EntityType = entity.GetType();
            ProxyEntity = proxyEntity;

            if (proxyEntity != null)
            {
                var props = EntityType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).Where(op => op.PropertyType.IsPrimitiveType());
                foreach (var prop in props)
                {
                    this.OriginalValues[prop] = prop.GetValue(entity);
                }
            }
        }
    }
}
