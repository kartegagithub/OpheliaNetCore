using System;

namespace Ophelia.Data.Logging
{
    public class AuditLog
    {
        public long ID { get; set; }
        public long UserID { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public long EntityID { get; set; }
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public object? OldObject { get; set; }   
        public object? NewObject { get; set; }
        public Microsoft.EntityFrameworkCore.EntityState State { get; set; }
        public long ParentAuditLogID { get; set; }
    }
}
