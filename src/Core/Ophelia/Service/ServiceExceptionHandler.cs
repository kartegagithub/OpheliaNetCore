using System;

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
