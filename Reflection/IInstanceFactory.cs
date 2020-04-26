using System;

namespace Ophelia.Reflection
{
    public interface IInstanceFactory
    {
        object GetInstance(Type type);
        TInstance GetInstance<TInstance>();
    }
}
