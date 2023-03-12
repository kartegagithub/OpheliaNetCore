//using IInterceptor = Castle.DynamicProxy.IInterceptor;
//using Castle.DynamicProxy;
//using Microsoft.EntityFrameworkCore.Metadata;
//using System;
//using System.Linq;

//namespace Ophelia.Data.Model.Proxy
//{
//    internal class PropertyChangedInterceptor : IInterceptor
//    {
//        public bool TrackChanges { get; set; } = false;
//        public void Intercept(Castle.DynamicProxy.IInvocation invocation)
//        {
//            if (this.TrackChanges && invocation.Proxy is ITrackedEntity)
//            {
//                var methodName = invocation.Method.Name;
//                //if (methodName.StartsWith("set_", StringComparison.Ordinal))
//                //{
//                //    (invocation.Proxy as IPocoEntityTracker).EntityTracker.OnPropertyChanged(invocation.Method.Name, invocation.Arguments.FirstOrDefault());
//                //}
//            }
//            invocation.Proceed();
//        }
//    }
//}
