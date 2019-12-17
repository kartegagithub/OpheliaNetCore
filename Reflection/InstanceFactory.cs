using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Reflection
{
    public class InstanceFactory : IInstanceFactory
    {
        private static IInstanceFactory _Current;

        public TInstance GetInstance<TInstance>()
        {
            return (TInstance)this.GetInstance(typeof(TInstance));

        }
        public object GetInstance(Type type)
        {
            type = GetRealType(type);
            var constructor = type.GetConstructors().First();
            var parameters = constructor.GetParameters();

            if (!parameters.Any()) return Activator.CreateInstance(type);
            var args = new List<object>();
            foreach (var parameter in parameters)
            {
                var arg = GetInstance(parameter.ParameterType);
                args.Add(arg);
            }
            var result = Activator.CreateInstance(type, args.ToArray());
            return result;

        }

        protected ConstructorInfo GetDefaultConstructor<TInstance>()
        {
            ConstructorInfo defaultConstructor = typeof(TInstance).GetConstructor(Type.EmptyTypes);
            return defaultConstructor;
        }

        protected Type GetRealType(Type type)
        {
            return Assembly.GetAssembly(type).GetExportedTypes()
                .Where(type.IsAssignableFrom)
                .Where(t => !t.IsAbstract)
                .Where(t => !t.IsInterface)
                .First();
        }

        public static IInstanceFactory Current
        {
            get
            {
                _Current = _Current ?? new InstanceFactory();
                return _Current;
            }
        }
    }
}
