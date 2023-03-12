using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Ophelia.Data
{
    public class DataProtector
    {
        public string ProtectionKey { get; set; }
        public virtual void OnSave(object entity)
        {
            this.Protect(entity);
        }
        public virtual void Unprotect<T>(T entity, Expression<Func<T, object>> field)
        {
            if (entity == null || (field as MemberExpression) == null)
                return;

            var memberExpression = (field as MemberExpression);
            var p = memberExpression.Member as PropertyInfo;
            if (p != null && p.CanRead && p.CanWrite)
                this.UnprotectValueAndSet(entity, p);
        }
        public virtual void Unprotect<T>(IEnumerable<T> entities)
        {
            var props = this.GetProtectedProperties(typeof(T));
            foreach (var entity in entities)
            {
                foreach (var p in props)
                {
                    if (p != null && p.CanRead && p.CanWrite)
                        this.UnprotectValueAndSet(entity, p);
                }
            }
        }
        public virtual void Unprotect<T>(T entity)
        {
            var props = this.GetProtectedProperties(entity.GetType());
            foreach (var p in props)
            {
                if (p != null && p.CanRead && p.CanWrite)
                    this.UnprotectValueAndSet(entity, p);
            }
        }
        public virtual void Protect<T>(T entity)
        {
            var props = this.GetProtectedProperties(entity.GetType());
            foreach (var p in props)
            {
                if (p != null && p.CanRead && p.CanWrite)
                    this.ProtectValueAndSet(entity, p);
            }
        }
        public virtual void Protect<T>(T entity, Expression<Func<T, object>> field)
        {
            if (entity == null || (field as MemberExpression) == null)
                return;

            var memberExpression = (field as MemberExpression);
            var p = memberExpression.Member as PropertyInfo;
            if (p != null && p.CanRead && p.CanWrite)
                this.ProtectValueAndSet(Convert.ToString(p.GetValue(entity)), p);
        }
        protected virtual string ProtectValue(string value, PropertyInfo p = null)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            if (value.StartsWith("ENC_"))
                return value;
            return $"ENC_{value.Encrypt(this.ProtectionKey)}";
        }
        protected virtual void ProtectValueAndSet(object entity, PropertyInfo p = null)
        {
            p.SetValue(entity, this.ProtectValue(Convert.ToString(p.GetValue(entity)), p));
        }
        protected virtual string UnprotectValue(string value, PropertyInfo p = null)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            if (!value.StartsWith("ENC_"))
                return value;
            return value.Replace("ENC_", "").Decrypt(this.ProtectionKey);
        }
        protected virtual void UnprotectValueAndSet(object entity, PropertyInfo p = null)
        {
            p.SetValue(entity, this.UnprotectValue(Convert.ToString(p.GetValue(entity)), p));
        }
        protected IEnumerable<PropertyInfo> GetProtectedProperties(Type entityType)
        {
            return entityType.GetProperties().Where(op => op.CanWrite && op.CanRead && op.CustomAttributes.Count() > 0 && op.CustomAttributes.Where(op2 => op2.AttributeType == typeof(Data.Attributes.GDPFieldAttribute)).Any());
        }
    }
}
