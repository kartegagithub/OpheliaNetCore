using AngleSharp.Dom;
using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Ophelia.Data.Model.Proxy
{
    internal class InternalProxyGenerator : Castle.DynamicProxy.ProxyGenerator
    {
        public InternalProxyGenerator() { }

        public static object CreateWithTarget(Type type, object target)
        {
            var instance = new InternalProxyGenerator();
            var proxyEntity = instance.CreateClassProxyWithTarget(type, new Type[] { typeof(ITrackedEntity) }, target, new TrackedEntityInterceptor(target));
            return proxyEntity;
        }
    }
}
