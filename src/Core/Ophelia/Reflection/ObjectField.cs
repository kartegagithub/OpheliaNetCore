using System;
using System.Reflection;

namespace Ophelia.Reflection
{
    public class ObjectField
    {
        public PropertyInfo FieldProperty { get; set; }
        public PropertyInfo MappedProperty { get; set; }
    }
}
