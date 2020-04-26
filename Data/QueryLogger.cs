using System;
using System.Collections.Generic;

namespace Ophelia.Data
{

    public class QueryLogger : IDisposable
    {
        public List<Model.SQLLog> SQLLogs { get; private set; }
        public List<Model.EntityLoadLog> EntityLoadLogs { get; private set; }
        public void LogSQL(Model.SQLLog log)
        {
            this.SQLLogs.Add(log);
        }
        public void LogLoad(Model.EntityLoadLog log)
        {
            this.EntityLoadLogs.Add(log);
        }
        public virtual void Dispose()
        {
            this.SQLLogs = null;
            this.EntityLoadLogs = null;
        }

        public QueryLogger()
        {
            this.SQLLogs = new List<Model.SQLLog>();
            this.EntityLoadLogs = new List<Model.EntityLoadLog>();
        }
    }
}
