using System;
using System.Linq;
using System.Diagnostics;

namespace Ophelia.Data.Model.Proxy
{
    internal class InternalProxyGenerator : Castle.DynamicProxy.ProxyGenerator
    {
        public InternalProxyGenerator() { }

        public static object Create(Type type, object source)
        {
            var instance = new InternalProxyGenerator();

            object proxyEntity = null;
            if (!typeof(ITrackedEntity).IsAssignableFrom(type))
            {
                //TODO: Existing runtime type will be resolved and reused.
                proxyEntity = instance.CreateClassProxy(type, new Type[] { typeof(ITrackedEntity) }, new TrackedEntityInterceptor(source));
            }
            else
            {
                proxyEntity = Activator.CreateInstance(type);
                var entity = (proxyEntity as ITrackedEntity);
                if(entity.Tracker == null)
                    entity.Tracker = new PocoEntityTracker(source, proxyEntity);
                else
                {
                    entity.Tracker.Entity = source;
                    entity.Tracker.ProxyEntity = proxyEntity;
                }
            }

            source.CopyTo(proxyEntity, "Tracker");
            (proxyEntity as ITrackedEntity).Tracker.ResetOriginalValues();
            return proxyEntity;
        }
    }
}
