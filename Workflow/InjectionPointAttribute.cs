using System;

namespace Ophelia.Workflow
{
    public class InjectionPointAttribute : Attribute
    {
        public string MethodName { get; set; }
        public Type EntityType { get; set; }
        public System.Reflection.MethodInfo Method { get; set; }
        public InjectionPointAttribute()
        {

        }
    }
}