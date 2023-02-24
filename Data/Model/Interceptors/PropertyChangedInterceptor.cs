using IInterceptor = Castle.DynamicProxy.IInterceptor;
using Castle.DynamicProxy;
using Microsoft.EntityFrameworkCore.Metadata;
using System;

namespace Ophelia.Data.Model.Interceptors
{
    internal class PropertyChangedInterceptor : IInterceptor
    {
        public bool TrackChanges { get; set; } = false;
        public void Intercept(Castle.DynamicProxy.IInvocation invocation)
        {
            if (this.TrackChanges)
            {
                var methodName = invocation.Method.Name;
                if (methodName.StartsWith("set_", StringComparison.Ordinal))
                {
                    var newValue = invocation.Arguments[0];
                }
            }
            invocation.Proceed();
        }
    }
}
