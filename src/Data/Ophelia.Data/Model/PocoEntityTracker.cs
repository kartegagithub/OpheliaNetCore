using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace Ophelia.Data.Model
{
    public class PocoEntityTracker
    {
        internal object Entity { get; set; }
        internal object ProxyEntity { get; set; }
        internal Type EntityType { get; set; }
        internal Dictionary<PropertyInfo, object> OriginalValues { get; set; }
        internal List<DataValue> Changes { get; set; }
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
            var entity = this.ProxyEntity;
            if (entity == null)
                entity = this.Entity;

            var pks = Extensions.GetPrimaryKeyProperties(this.EntityType);
            foreach (var pk in pks)
            {
                if (pk.PropertyType.GetDefaultValue().Equals(pk.GetValue(entity)))
                    return true;
            }
            return false;
        }
        public virtual List<DataValue> GetChanges()
        {
            var list = new List<DataValue>();
            foreach (var item in this.OriginalValues)
            {
                var isNotMapped = item.Key.GetCustomAttributes(typeof(NotMappedAttribute)).Any();
                if (isNotMapped)
                    continue;

                var value = item.Key.GetValue(this.ProxyEntity);
                if (value == null && item.Value == null)
                    continue;

                if (Extensions.IsIdentityProperty(item.Key))
                    continue;

                if (!item.Key.PropertyType.IsNullable() && value == null)
                    value = item.Key.PropertyType.GetDefaultValue();

                if (!this.IsNewRecord())
                {
                    if ((value != null && item.Value == null) || (value == null && item.Value != null) || (!value.Equals(item.Value)))
                        list.Add(new DataValue() { PropertyInfo = item.Key, Value = value });

                }
                else
                    list.Add(new DataValue() { PropertyInfo = item.Key, Value = value });
            }
            this.Changes = list;
            this._HasChanged = list.Any();
            return list;
        }
        internal virtual void OnAfterCreateEntity(bool processDefaults = true)
        {
            if (ProxyEntity == null)
                ProxyEntity = Entity;
            Entity = Activator.CreateInstance(ProxyEntity.GetType());
            ProxyEntity.CopyTo(Entity, "Tracker");
            this.ResetOriginalValues();
        }
        internal virtual void OnBeforeUpdateEntity(bool processDefaults = true, DateTimeKind kind = DateTimeKind.Local)
        {

        }
        internal virtual void OnBeforeInsertEntity(bool processDefaults = true, DateTimeKind kind = DateTimeKind.Local)
        {

        }
        internal virtual void OnAfterUpdateEntity(bool processDefaults = true, DateTimeKind kind = DateTimeKind.Local)
        {
            Entity = Activator.CreateInstance(ProxyEntity.GetType());
            ProxyEntity.CopyTo(Entity, "Tracker");
            this.ResetOriginalValues();
        }

        internal virtual void OnAfterDeleteEntity()
        {

        }
        internal void ResetOriginalValues()
        {
            this.OriginalValues = new Dictionary<PropertyInfo, object>();
            if (ProxyEntity != null)
            {
                var props = EntityType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).Where(op => op.PropertyType.IsPrimitiveType());
                foreach (var prop in props)
                {
                    if (this.OriginalValues.Any(op => op.Key == prop))
                        this.OriginalValues[prop] = prop.GetValue(Entity);
                    else
                        this.OriginalValues.Add(prop, prop.GetValue(Entity));
                    //this.OriginalValues[prop] = prop.GetValue(entity);
                }
            }
        }
        public PocoEntityTracker(object entity, object proxyEntity)
        {
            Entity = entity;
            EntityType = entity.GetType();
            ProxyEntity = proxyEntity;
        }
    }
}
