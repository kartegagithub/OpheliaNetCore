using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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

            if (!configAttributes.IsNullOrEmpty<object>())
                result = (T)configAttributes[0];

            return result;
        }
    }
}
