using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq.Expressions;
namespace Ophelia.Data.Model
{
    internal class DataEntityTracker : IDisposable
    {
        private DataEntity Entity;
        internal bool LoadAnyway { get; set; }
        internal bool HasChanged { get; set; }
        internal EntityState State { get; set; }
        internal Dictionary<string, DataValue> Properties { get; set; }

        internal TEntity GetEntityValue<TEntity>(PropertyInfo property)
        {
            if (!property.PropertyType.IsDataEntity())
                throw new Exception("Expressions are allowed only for DataEntity subclasses");

            var key = property.Name;
            if (DataContext.Current.Configuration.EnableLazyLoading || this.LoadAnyway)
            {
                bool canCreate = !this.Properties.Keys.Contains(key);
                if (canCreate)
                {
                    var dataset = (QueryableDataSet<TEntity>)property.PropertyType.CreateDataSet();
                    var refProp = this.Entity.GetType().GetProperties().Where(op => op.Name == property.Name + "ID").FirstOrDefault();
                    var idProp = property.PropertyType.GetProperties().Where(op => op.Name == "ID").FirstOrDefault();

                    this.Properties[key] = new DataValue { PropertyInfo = property, Value = dataset.Where(idProp, refProp.GetValue(this.Entity)).FirstOrDefault() };
                    this.Properties[key].HasChanged = false;
                }
            }
            if (this.Properties.Keys.Contains(key))
                return (TEntity)this.Properties[key].Value;
            else
                return default(TEntity);
        }
        internal System.Collections.IEnumerable GetCollectionValue<TEntity>(PropertyInfo property)
        {
            if (!property.PropertyType.IsGenericType)
                throw new Exception("Expressions are allowed only for ICollection subclasses");

            var key = property.Name;
            bool canCreate = !this.Properties.Keys.Contains(key);
            if (canCreate)
            {
                var list = (System.Collections.IEnumerable)property.PropertyType.GenericTypeArguments[0].CreateList();
                this.Properties[key] = new DataValue { PropertyInfo = property, Value = list };
                this.Properties[key].HasChanged = false;
            }
            if (this.Properties.Keys.Contains(key))
                return (System.Collections.IEnumerable)this.Properties[key].Value;
            else
                return default(System.Collections.IEnumerable);
        }
        internal QueryableDataSet<TEntity> GetDataSetValue<TEntity>(PropertyInfo property, Expression<Func<TEntity, bool>> predicate)
            where TEntity : class
        {
            if (!property.PropertyType.IsQueryableDataSet())
                throw new Exception("Expressions are allowed only for QueryableDataSet subclasses");

            var key = property.Name;
            if (DataContext.Current.Configuration.EnableLazyLoading || this.LoadAnyway)
            {
                bool canCreate = !this.Properties.Keys.Contains(key);
                if (canCreate)
                {
                    var dataset = (QueryableDataSet<TEntity>)this.GetValue(property);

                    var entityType = property.PropertyType.GenericTypeArguments[0];
                    System.Reflection.PropertyInfo refProp = null;

                    var attributes = property.GetCustomAttributes(true);
                    if (attributes.Length > 0)
                    {
                        Attributes.RelationFKProperty pkProperty = (Attributes.RelationFKProperty)attributes.Where(op => op.GetType().IsAssignableFrom(typeof(Attributes.RelationFKProperty))).FirstOrDefault();
                        if (pkProperty != null)
                            refProp = entityType.GetProperty(pkProperty.PropertyName);

                        var filterProperties = attributes.Where(op => op.GetType().IsAssignableFrom(typeof(Attributes.RelationFilterProperty))).ToList();
                        if (filterProperties != null && filterProperties.Count > 0)
                        {
                            foreach (Attributes.RelationFilterProperty item in filterProperties)
                            {
                                dataset = dataset.Where(entityType.GetProperty(item.PropertyName), item.Value);
                            }
                        }
                        var n2nRelationProperties = attributes.Where(op => op.GetType().IsAssignableFrom(typeof(Attributes.N2NRelationProperty))).ToList();
                        if (n2nRelationProperties != null && n2nRelationProperties.Count > 0)
                        {
                            foreach (Attributes.N2NRelationProperty item in n2nRelationProperties)
                            {
                                var subQuery = item.RelationClassType.CreateDataSet();
                                item.FilterValue = this.Entity.ID;
                                if (string.IsNullOrEmpty(item.FilterName))
                                    item.FilterName = this.Entity.GetType().Name + "ID";
                                if (subQuery is Model.QueryableDataSet)
                                    dataset = dataset.Where(new Expressions.InExpression((subQuery as Model.QueryableDataSet).GetSourceExpression(), item));
                                else
                                    dataset = dataset.Where(new Expressions.InExpression((subQuery as IQueryable).Expression, item));
                            }
                        }
                    }
                    if (refProp == null)
                        refProp = entityType.GetProperties().Where(op => op.Name == this.Entity.GetType().Name + "ID").FirstOrDefault();

                    if (predicate != null)
                        dataset = dataset.Where(predicate);

                    if (refProp != null)
                    {
                        this.Properties[key].Value = dataset.Where(refProp, this.Entity.ID);
                    }
                    else
                    {
                        this.Properties[key].Value = dataset;
                    }
                    this.Properties[key].HasChanged = false;
                }
            }
            if (this.Properties.Keys.Contains(key))
                return (QueryableDataSet<TEntity>)this.Properties[key].Value;
            else
                return null;
        }
        internal object GetValue(PropertyInfo property, object defaultValue = null)
        {
            var key = property.Name;
            try
            {
                this.CheckKey(property, defaultValue);
                return this.Properties[key].Value;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                key = "";
            }
        }

        public void SetValue(PropertyInfo property, object Value)
        {
            var key = property.Name;
            try
            {
                this.CheckKey(property);
                this.Properties[key].Value = Value;
                if (this.State == EntityState.Loading)
                    this.Properties[key].HasChanged = false;
                this.HasChanged = property.Name != "ID" && this.State == EntityState.Loaded && (this.HasChanged || this.Properties[key].HasChanged);
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                key = "";
            }
        }

        private void CheckKey(PropertyInfo property, object defaultValue = null)
        {
            var key = property.Name;
            try
            {
                if (!this.Properties.Keys.Contains(key))
                {
                    object value = null;
                    if (defaultValue != null)
                    {
                        value = defaultValue;
                    }
                    else
                    {
                        if (property.PropertyType.IsValueType && Nullable.GetUnderlyingType(property.PropertyType) == null)
                            value = Activator.CreateInstance(property.PropertyType);
                        else if (property.PropertyType.IsDataEntity())
                        {
                            value = property.PropertyType.CreateDataSet();
                        }
                        else if (property.PropertyType.IsQueryableDataSet())
                        {
                            value = property.PropertyType.GenericTypeArguments[0].CreateDataSet();
                        }
                    }
                    this.Properties[key] = new DataValue { PropertyInfo = property, Value = value };
                    this.Properties[key].HasChanged = false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                key = "";
            }
        }

        internal void OnAfterCreateEntity()
        {

        }
        internal void OnBeforeUpdateEntity()
        {

        }
        internal void OnBeforeInsertEntity()
        {

        }
        internal void OnAfterUpdateEntity()
        {

        }

        internal void OnAfterDeleteEntity()
        {

        }
        internal List<DataValue> GetChanges()
        {
            if (this.Entity.ID > 0)
                return this.Properties.Where(op => op.Value.HasChanged && op.Value.PropertyInfo.Name != "ID").Select(op => op.Value).ToList();
            else
                return this.Properties.Where(op => op.Value.PropertyInfo.Name != "ID").Select(op => op.Value).ToList();
        }

        public DataEntityTracker(DataEntity entity)
        {
            this.Entity = entity;
            this.Properties = new Dictionary<string, DataValue>();
        }

        public void Dispose()
        {
            this.Entity = null;
            this.Properties.Clear();
            this.Properties = null;
            GC.SuppressFinalize(this);
        }
    }
}
