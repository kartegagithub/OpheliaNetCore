using System;

namespace Ophelia.Data.Logging
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AuditLoggingAttribute : Attribute
    {
        public bool Enable { get; set; }
        public bool ParentOfPostActions { get; set; }
        public string ParentPropertyName { get; set; } = string.Empty;
        public AuditLoggingAttribute(bool enable) => this.Enable = enable;
        public AuditLoggingAttribute() => this.Enable = true;
    }
}
