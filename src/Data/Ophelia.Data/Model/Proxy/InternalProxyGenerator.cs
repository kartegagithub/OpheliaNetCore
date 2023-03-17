using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace Ophelia.Data.Model.Proxy
{
    internal class InternalProxyGenerator : Castle.DynamicProxy.ProxyGenerator
    {
        public InternalProxyGenerator() { }
        private static Dictionary<Type, Type> CLRTypeCache { get; set; } = new Dictionary<Type, Type>();

        public static object? Create(Type type, object source)
        {
            var instance = new InternalProxyGenerator();

            ITrackedEntity? proxyEntity = null;
            Type? clrType = null;

            //If entity type is not ITrackedEntity, system will generate a new ITrackedEntity which inherits entity.
            if (!typeof(ITrackedEntity).IsAssignableFrom(type))
            {
                //If type already exists in cache
                if (!CLRTypeCache.TryGetValue(type, out clrType))
                    clrType = type.GetRealTypes(false).Where(op => typeof(ITrackedEntity).IsAssignableFrom(type) && op.Assembly.IsDynamic).FirstOrDefault();

                //If not, a new class will be generated and new type is cached
                if (clrType == null)
                {
                    proxyEntity = (ITrackedEntity)instance.CreateClassProxy(type, new Type[] { typeof(ITrackedEntity) }, new TrackedEntityInterceptor(source));
                    CLRTypeCache[type] = proxyEntity.GetType();
                    Ophelia.ReflectionExtensions.RemoveTypeCache(type);
                }
                else //Otherwise system creates a new instance of pre-generated proxy type.
                    proxyEntity = (ITrackedEntity)Activator.CreateInstance(clrType);

                if (proxyEntity != null)
                {
                    //To track changes, all property values will be dublicated
                    source.CopyTo(proxyEntity, "Tracker");
                    proxyEntity.Tracker.ResetOriginalValues();
                }
            }
            else
            {
                //If entity is Ophelia DataEntity, tracking will be internal, so just creating entity and returning.
                if (type.IsDataEntity())
                {
                    proxyEntity = (ITrackedEntity)Activator.CreateInstance(type);
                }
                else
                {
                    if (!typeof(ITrackedEntity).IsAssignableFrom(type))
                    {
                        string message = $"'{type.FullName}' is not implementing Ophelia.Data.Model.Proxy.ITrackedEntity or is not inheriting Ophelia DataEntity";
                        throw new ProxyGenerationException(message);
                    }

                    //If entity type is implementing ITrackingEntity, then new instance is created and Tracker prop is set.
                    proxyEntity = (ITrackedEntity)Activator.CreateInstance(type);
                    if (proxyEntity != null)
                    {
                        if (proxyEntity.Tracker == null)
                            proxyEntity.Tracker = new PocoEntityTracker(source, proxyEntity);
                        else
                        {
                            proxyEntity.Tracker.Entity = source;
                            proxyEntity.Tracker.ProxyEntity = proxyEntity;
                        }
                        source.CopyTo(proxyEntity, "Tracker");
                        proxyEntity.Tracker.ResetOriginalValues();
                    }
                }
            }
            return proxyEntity;
        }
    }
    public class ProxyGenerationException : Exception
    {
        public ProxyGenerationException(string message) : base(message)
        {

        }
    }
}
