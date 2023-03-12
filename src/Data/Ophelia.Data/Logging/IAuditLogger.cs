using System;
using System.Collections.Generic;

namespace Ophelia.Data.Logging
{
    public interface IAuditLogger : IDisposable
    {
        void Write(List<AuditLog> logs);
    }
}
