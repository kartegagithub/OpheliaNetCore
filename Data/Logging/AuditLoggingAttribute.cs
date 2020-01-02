using System;
using System.Collections.Generic;
using System.Text;

namespace Ophelia.Data.Logging
{
    public class AuditLoggingAttribute : Attribute
    {
        public bool Enable { get; set; }
        public AuditLoggingAttribute(bool enable)
        {
            this.Enable = enable;
        }
        public AuditLoggingAttribute()
        {
            this.Enable = true;
        }
    }
}
