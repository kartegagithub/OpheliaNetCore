using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ophelia.Reflection
{
    public interface IInstanceFactory
    {
        object GetInstance(Type type);
        TInstance GetInstance<TInstance>();
    }
}
