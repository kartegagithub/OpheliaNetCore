using Ophelia.Data.Model;
using System;
using System.Linq;
using System.Reflection;

namespace Ophelia.Data
{
    public class Repository : IDisposable
    {
        public DataContext Context { get; private set; }

        public bool Delete(object entity)
        {
            return this.Context.CreateDeleteQuery(entity).Execute<int>() > 0;
        }

        public bool SaveChanges(object entity)
        {
            return this.SaveChanges(entity, true);
        }
        public bool SaveChanges(object entity, bool runBeforeUpdateProcesseses)
        {
            var tracker = (entity.GetPropertyValue("Tracker") as PocoEntityTracker);
            if (tracker != null && (tracker.HasChanged || tracker.IsNewRecord()))
            {
                int effectedRowCount = 0;
                if (!tracker.IsNewRecord())
                {
                    tracker?.OnBeforeUpdateEntity(runBeforeUpdateProcesseses);
                    effectedRowCount = this.Context.CreateUpdateQuery(entity).Execute<int>();
                    tracker?.OnAfterUpdateEntity(runBeforeUpdateProcesseses);
                }
                else
                {
                    tracker?.OnBeforeInsertEntity(runBeforeUpdateProcesseses);

                    effectedRowCount = this.Context.CreateInsertQuery(entity).Execute<int>();
                    if (this.Context.Connection.Type == DatabaseType.MySQL || this.Context.Connection.Type == DatabaseType.SQLServer || this.Context.Connection.Type == DatabaseType.PostgreSQL)
                    {
                        var pkMethod = Extensions.GetPrimaryKeyProperty(entity.GetType());
                        pkMethod.SetValue(entity, effectedRowCount);
                    }
                    tracker?.OnAfterCreateEntity(runBeforeUpdateProcesseses);
                }

                var entityType = entity.GetType();
                var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(op => op.PropertyType.IsPOCOEntity()).ToList();
                if (properties.Count > 0)
                {
                    foreach (var _prop in properties)
                    {
                        var referenced = _prop.GetValue(entity);
                        if (referenced != null)
                        {
                            var innerTracker = (referenced.GetPropertyValue("Tracker") as PocoEntityTracker);
                            if (innerTracker != null && (innerTracker.HasChanged || innerTracker.IsNewRecord()))
                            {
                                this.SaveChanges(referenced);

                                // Example: Entity (Student) has School prop. When saving new Student, School is saved/created and then SchoolID is set to Student.
                                var refMethod = Extensions.GetForeignKeyProp(_prop).Item1;
                                var pkMethod = Extensions.GetPrimaryKeyProperty(referenced.GetType());
                                if (refMethod != null && refMethod.GetValue(entity) != pkMethod.GetValue(referenced))
                                {
                                    refMethod.SetValue(entity, pkMethod.GetValue(referenced));

                                    tracker?.OnBeforeUpdateEntity();
                                    this.Context.CreateUpdateQuery(entity).Execute<int>();
                                    tracker?.OnAfterUpdateEntity();
                                }
                            }
                        }
                        referenced = null;
                    }
                }

                properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(op => op.PropertyType.IsQueryableDataSet() || op.PropertyType.IsQueryable()).ToList();
                if (properties.Count > 0)
                {
                    foreach (var _prop in properties)
                    {
                        var referencedCollection = (System.Collections.IEnumerable)_prop.GetValue(entity);
                        if (referencedCollection != null)
                        {
                            var attributes = _prop.GetCustomAttributes(true);
                            var n2nRelationProperties = attributes.Where(op => op.GetType().IsAssignableFrom(typeof(Attributes.N2NRelationProperty))).ToList();
                            if (n2nRelationProperties != null && n2nRelationProperties.Count > 0)
                            {

                            }
                            else
                            {
                                foreach (var referenced in referencedCollection)
                                {
                                    var innerTracker = (referenced.GetPropertyValue("Tracker") as PocoEntityTracker);
                                    if (innerTracker != null && (innerTracker.HasChanged || innerTracker.IsNewRecord()))
                                    {
                                        var refMethod = Extensions.GetForeignKeyProp(referenced.GetType(), entityType.Name);
                                        if (refMethod.Item2)
                                        {
                                            var pkMethod = Extensions.GetPrimaryKeyProperty(entity.GetType());
                                            refMethod.Item1.SetValue(referenced, pkMethod.GetValue(entity));
                                        }
                                        this.SaveChanges(referenced);
                                    }
                                }
                            }
                        }
                    }
                }
                return effectedRowCount > 0;
            }
            return false;
        }
        public object Track(object entity)
        {
            return Model.Proxy.InternalProxyGenerator.Create(entity.GetType(), entity);
        }
        public Repository(DataContext context)
        {
            this.Context = context;
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {

        }
    }
}
