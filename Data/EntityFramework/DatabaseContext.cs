using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Ophelia.Data.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ophelia.Data.EntityFramework
{
    public class DatabaseContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public IConfiguration Configuration { get; private set; }
        public DbContextOptions Options { get; private set; }
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
        public override int SaveChanges()
        {
            this.CheckAuditState();
            return base.SaveChanges();
        }
        protected virtual void CheckAuditState()
        {
            if (!this.EnableAuditLog)
                return;

            //changeSet method that examines changes
            var changeSet = this.ChangeTracker.Entries();
            var logs = new List<AuditLog>();

            //where the value is considered to be null. So even if there is no change in the context, it can be null.
            if (changeSet != null)
            {
                //The model to send to the log table in db.

                //multiple classes may have changed. We will consider the unchanged and undeleted ones.
                foreach (var entry in changeSet.Where(c => c.State != Microsoft.EntityFrameworkCore.EntityState.Unchanged))
                {
                    var attributes = entry.Entity.GetType().GetCustomAttributes(typeof(AuditLoggingAttribute));
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
                        string key = (string)item.Name;

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
                            string key = (string)item.Name;

                            //We find the value of entity
                            var value = "";
                            if (entry.CurrentValues[item] != null)
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
                }
            }
            this.WriteAuditLogs(logs);
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
        }
    }
}
