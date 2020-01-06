using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Service
{
    public abstract class ServiceExceptionHandler
    {
        public abstract void HandleException(Exception ex);
        public static ServiceExceptionHandler CreateInstance()
        {
            return ((ServiceExceptionHandler)typeof(ServiceExceptionHandler).GetRealTypeInstance(false));
        }
    }
}
