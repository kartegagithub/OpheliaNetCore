using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Ophelia.Data.Attributes;
using Ophelia.Data.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ophelia.Data.EntityFramework
{
    public class DatabaseContext : Microsoft.EntityFrameworkCore.DbContext
    {
        private bool _IsDisposed;
        public DataProtector DataProtector { get; set; }
        public override void Dispose()
        {
            this._IsDisposed = true;
            this.PostActionAudits = null;
            base.Dispose();
        }
        public bool IsDisposed
        {
            get
            {
                return this._IsDisposed;
            }
        }
        public IConfiguration Configuration { get; private set; }
        public DbContextOptions Options { get; private set; }
        public Dictionary<string, long> PostActionAudits { get; private set; }
        public bool EnableAuditLog { get; set; }
        public TEntity Create<TEntity>() { return (TEntity)Activator.CreateInstance(typeof(TEntity)); }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            var types = typeof(Ophelia.Data.EntityFramework.IEntityConfigurator).GetAssignableClasses();
            foreach (var type in types)
            {
                ((Ophelia.Data.EntityFramework.IEntityConfigurator)Activator.CreateInstance(type)).Configure(builder);
            }
        }
        public override EntityEntry Add(object entity)
        {
            return base.Add(entity);
        }
        public override EntityEntry<TEntity> Add<TEntity>(TEntity entity)
        {
            return base.Add(entity);
        }
        public override int SaveChanges()
        {
            this.OnBeforeSaveChanges();
            this.CheckAuditState();
            return base.SaveChanges();
        }
        protected virtual void OnBeforeSaveChanges()
        {
            var changeSet = this.ChangeTracker.Entries();
            if (changeSet != null)
            {
                foreach (var entry in changeSet.Where(c => c.State != Microsoft.EntityFrameworkCore.EntityState.Unchanged))
                {
                    this.OnEntityChange(entry);
                }
            }
        }
        protected virtual void CheckAuditState()
        {
            if (!this.EnableAuditLog)
                return;

            //changeSet method that examines changes
            var changeSet = this.ChangeTracker.Entries();
            //where the value is considered to be null. So even if there is no change in the context, it can be null.
            if (changeSet != null)
            {
                //The model to send to the log table in db.

                //multiple classes may have changed. We will consider the unchanged and undeleted ones.
                foreach (var entry in changeSet.Where(c => c.State != Microsoft.EntityFrameworkCore.EntityState.Unchanged))
                {
                    List<object> attributes = null;
                    if (this.DataProtector != null)
                    {
                        attributes = entry.Entity.GetType().GetCustomAttributes(typeof(GDPClassAttribute));
                        if (attributes != null && attributes.Count > 0)
                            this.DataProtector.OnSave(entry.Entity);
                    }

                    var logs = new List<AuditLog>();
                    attributes = entry.Entity.GetType().GetCustomAttributes(typeof(AuditLoggingAttribute));
                    if (attributes == null || attributes.Count == 0 || !(attributes.FirstOrDefault() as AuditLoggingAttribute).Enable)
                        continue;

                    var auditLogModel = new AuditLog()
                    {
                        EntityName = entry.Entity.GetType().Name,
                        EntityID = entry.State == Microsoft.EntityFrameworkCore.EntityState.Added ? 0 : Convert.ToInt64(entry.Entity.GetPropertyValue("ID")),
                        UserID = Convert.ToInt64(entry.Entity.GetPropertyValue("UserCreatedID")),
                        Date = DateTime.Now,
                        State = entry.State
                    };
                    logs.Add(auditLogModel);

                    var changes = new Dictionary<string, string>();
                    foreach (var item in entry.CurrentValues.Properties)
                    {
                        //We find the name of the entity.
                        string key = item.Name;

                        //We find the value of entity
                        var value = "";
                        if (entry.CurrentValues[item] != null)
                            value = entry.CurrentValues[item].ToString();

                        if (key != null && value != null)
                        {
                            changes.Add(key, value);
                        }
                    }
                    var newValue = JsonConvert.SerializeObject(changes);
                    auditLogModel.NewValue = newValue;
                    auditLogModel.NewObject = entry.CurrentValues;

                    if (entry.OriginalValues.Properties.Count > 0 && entry.State != Microsoft.EntityFrameworkCore.EntityState.Added)
                    {
                        changes = new Dictionary<string, string>();
                        foreach (var item in entry.OriginalValues.Properties)
                        {
                            //We find the name of the entity.
                            string key = item.Name;

                            //We find the value of entity
                            var value = "";
                            if (entry.OriginalValues[item] != null)
                                value = entry.OriginalValues[item].ToString();

                            if (key == "ID")
                            {
                                auditLogModel.EntityID = long.Parse(value);
                            }

                            if (key == "UserCreatedID")
                            {
                                auditLogModel.UserID = long.Parse(value);
                            }

                            changes.Add(key, value);
                        }

                        var originalValue = JsonConvert.SerializeObject(changes);
                        auditLogModel.OldValue = originalValue;
                        auditLogModel.OldObject = entry.OriginalValues;
                    }
                    if (!string.IsNullOrEmpty((attributes.FirstOrDefault() as AuditLoggingAttribute).ParentPropertyName))
                    {
                        var parentName = (attributes.FirstOrDefault() as AuditLoggingAttribute).ParentPropertyName;
                        var parentProperty = entry.Entity.GetType().GetProperty(parentName);
                        if (parentProperty != null)
                        {
                            var id = entry.Entity.GetPropertyValue(parentName + "ID");
                            if (id != null)
                            {
                                long longID = 0;
                                if (long.TryParse(id.ToString(), out longID))
                                {
                                    if (this.PostActionAudits.ContainsKey($"{parentProperty.PropertyType.Name}_{longID}"))
                                    {
                                        auditLogModel.ParentAuditLogID = this.PostActionAudits[$"{parentProperty.PropertyType.Name}_{longID}"];
                                    }
                                }
                            }

                        }

                    }
                    this.WriteAuditLogs(logs);
                    if (attributes != null && (attributes.FirstOrDefault() as AuditLoggingAttribute).ParentOfPostActions)
                    {
                        this.PostActionAudits[$"{auditLogModel.EntityName}_{auditLogModel.EntityID}"] = auditLogModel.ID;
                    }
                }
            }
        }
        protected virtual void OnEntityChange(EntityEntry entity)
        {

        }
        protected virtual void WriteAuditLogs(List<AuditLog> logs)
        {
            using (var Handler = (IAuditLogger)typeof(IAuditLogger).GetRealTypeInstance())
            {
                if (Handler != null)
                {
                    Handler.Write(logs);
                }
            }
        }
        public DatabaseContext(IConfiguration config, DbContextOptions options) : base(options)
        {
            this.Configuration = config;
            this.Options = options;
            this.PostActionAudits = new Dictionary<string, long>();
            this.DataProtector = new DataProtector();
        }
    }
}
