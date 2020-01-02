using System;
using System.Collections.Generic;
using System.Text;

namespace Ophelia.Data.Logging
{
    public interface IAuditLogger: IDisposable
    {
        void Write(List<AuditLog> logs);
    }
}
