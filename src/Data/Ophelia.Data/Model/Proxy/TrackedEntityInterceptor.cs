using System;
using IInterceptor = Castle.DynamicProxy.IInterceptor;

namespace Ophelia.Data.Model.Proxy
{
    internal class TrackedEntityInterceptor : IInterceptor
    {
        internal object Source { get; set; }
        public TrackedEntityInterceptor(object source)
        {
            this.Source = source;
        }
        private PocoEntityTracker Tracker { get; set; }
        public void Intercept(Castle.DynamicProxy.IInvocation invocation)
        {
            try
            {
                if (invocation.Method.Name == "get_Tracker")
                {
                    if (this.Tracker == null)
                        this.Tracker = new PocoEntityTracker(this.Source, invocation.Proxy);
                    invocation.ReturnValue = this.Tracker;
                }
                else
                    invocation.Proceed();
            }
            catch (Exception)
            {

            }
        }
    }
}
