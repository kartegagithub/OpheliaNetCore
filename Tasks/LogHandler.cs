using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ophelia.Tasks
{
    public abstract class LogHandler
    {
        public LogHandler() : base()
        {
        }

        public static LogHandler CreateInstance()
        {
            return ((LogHandler)typeof(LogHandler).GetRealTypeInstance(false));
        }
        public virtual void Write(string sender, string message, object data, List<string> dataMaskValues = null)
        {

        }
        public virtual void Write(string sender, object data, List<string> dataMaskValues = null)
        {

        }
    }
}
