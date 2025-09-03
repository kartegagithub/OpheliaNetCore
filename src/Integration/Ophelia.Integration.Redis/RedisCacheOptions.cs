using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Ophelia.Integration.Redis
{
    public class RedisCacheOptions : IOptions<RedisCacheOptions>
    {
        /// <summary>
        /// The command flags to use when reading data.
        /// </summary>
        public CommandFlags GetCommandFlags { get; set; } = CommandFlags.None;

        /// <summary>
        /// The configuration used to connect to Redis.
        /// </summary>
        public string Configuration { get; set; }

        /// <summary>
        /// The configuration used to connect to Redis.
        /// This is preferred over Configuration.
        /// </summary>
        public ConfigurationOptions ConfigurationOptions { get; set; }

        /// <summary>
        /// The Redis instance name.
        /// </summary>
        public string InstanceName { get; set; }

        RedisCacheOptions IOptions<RedisCacheOptions>.Value
        {
            get { return this; }
        }
        public RedisCacheOptions()
        {
            ConfigurationOptions = new ConfigurationOptions();
        }
    }
}
