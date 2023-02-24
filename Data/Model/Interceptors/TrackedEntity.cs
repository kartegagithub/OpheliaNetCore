using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Data.Model.Interceptors
{
    public interface ITrackedEntity
    {
        string EntityTracker { get; }
    }
}
