using System.Collections.Generic;

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
