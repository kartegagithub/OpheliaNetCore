using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Ophelia.Data
{
    public class Repository : IDisposable
    {
        private DataContext oContext;

        public DataContext Context
        {
            get
            {
                return this.oContext;
            }
        }

        public bool Delete(Model.DataEntity entity) 
        {
            if (entity.ID > 0)
            {
                int effectedRowCount = 0;
                effectedRowCount = this.Context.CreateDeleteQuery(entity).Execute<int>();
                return effectedRowCount > 0;
            }
            return false;
        }

        public bool SaveChanges(Model.DataEntity entity)
        {
            if (entity.ID == 0 || entity.Tracker.HasChanged)
            {
                int effectedRowCount = 0;
                if(entity.ID > 0)
                {
                    entity.Tracker.OnBeforeUpdateEntity();
                    entity.DateModified = DateTime.Now;
                    effectedRowCount = this.Context.CreateUpdateQuery(entity).Execute<int>();
                    entity.Tracker.OnAfterUpdateEntity();
                }
                else
                {
                    entity.Tracker.OnBeforeInsertEntity();
                    entity.DateModified = DateTime.Now;
                    entity.DateCreated = DateTime.Now;

                    effectedRowCount = this.Context.CreateInsertQuery(entity).Execute<int>();
                    if(entity.ID == 0 && (this.Context.Connection.Type == DatabaseType.MySQL || this.Context.Connection.Type == DatabaseType.SQLServer))
                        entity.ID = effectedRowCount;
                    entity.Tracker.OnAfterCreateEntity();
                }

                var entityType = entity.GetType();
                var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(op => op.PropertyType.IsDataEntity()).ToList();
                if(properties.Count > 0)
                {
                    foreach (var _prop in properties)
                    {
                        var referenced = (Model.DataEntity)_prop.GetValue(entity);
                        if(referenced != null)
                        {
                            if(referenced.ID == 0 || referenced.Tracker.HasChanged)
                            {
                                this.SaveChanges(referenced);

                                var refMethod = entityType.GetProperty(_prop.Name + "ID");
                                if(refMethod != null && (long)refMethod.GetValue(entity) != referenced.ID)
                                {
                                    refMethod.SetValue(entity, referenced.ID);

                                    entity.Tracker.OnBeforeUpdateEntity();
                                    this.Context.CreateUpdateQuery(entity).Execute<int>();
                                    entity.Tracker.OnAfterUpdateEntity();
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
                                    var oEntity = referenced as Model.DataEntity;
                                    if (oEntity != null)
                                    {
                                        if (oEntity.ID == 0 || oEntity.Tracker.HasChanged)
                                        {
                                            var refMethods = referenced.GetType().GetProperties().Where(op => op.Name == entityType.Name + "ID").ToList();
                                            if (refMethods.Count > 0)
                                            {
                                                if (refMethods.Count > 1)
                                                {
                                                    //TODO: Burada aynı tipte iki özellik varsa olacak karışıklık giderilecek.
                                                    //Örneğin: ProjectProgress üzerinde NodeID ve ResponsibleNodeID gibi dönüş tipleri aynı olanlar.
                                                }
                                                var refMethod = refMethods.FirstOrDefault();
                                                refMethod.SetValue(referenced, entity.ID);
                                            }
                                            this.SaveChanges(oEntity);
                                        }
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

        public Repository(DataContext Context)
        {
            this.oContext = Context;
        }

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
