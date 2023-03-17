using System;
using System.Reflection;

namespace Ophelia
{
    public static class AssemblyExtensions
    {
        public static T GetAttribute<T>(this Assembly callingAssembly)
            where T : Attribute
        {
            T result = null;

            object[] configAttributes = Attribute.GetCustomAttributes(callingAssembly,
                typeof(T), false);

            if (!configAttributes.IsNullOrEmpty())
                result = (T)configAttributes[0];

            return result;
        }
    }
}
