using IInterceptor = Castle.DynamicProxy.IInterceptor;
using Castle.DynamicProxy;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;

namespace Ophelia.Data.Model.Interceptors
{
    internal class TrackedEntityInterceptor : IInterceptor
    {
        public void Intercept(Castle.DynamicProxy.IInvocation invocation)
        {
            if (invocation.Method.Name == "get_EntityTracker")
                invocation.ReturnValue = "Test";
            else
                invocation.Proceed();
        }
    }
}
