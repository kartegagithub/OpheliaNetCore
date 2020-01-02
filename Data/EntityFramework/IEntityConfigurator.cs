using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ophelia.Data.EntityFramework
{
    /// <summary>
    /// Configurator
    /// </summary>
    public interface IEntityConfigurator
    {
        /// <summary>
        /// Configure method
        /// </summary>
        /// <param name="builder"></param>
        void Configure(ModelBuilder builder);
    }
}
