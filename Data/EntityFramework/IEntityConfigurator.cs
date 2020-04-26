using Microsoft.EntityFrameworkCore;

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
