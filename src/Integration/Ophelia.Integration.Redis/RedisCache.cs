using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace Ophelia.Integration.Redis
{
    public class RedisCache : IDistributedCache, IDisposable
    {
        [ThreadStatic]
        private static string _CacheKey;

        public static string CacheKey { get { return _CacheKey; } }

        // KEYS[1] = = key
        // ARGV[1] = absolute-expiration - ticks as long (-1 for none)
        // ARGV[2] = sliding-expiration - ticks as long (-1 for none)
        // ARGV[3] = relative-expiration (long, in seconds, -1 for none) - Min(absolute-expiration - Now, sliding-expiration)
        // ARGV[4] = data - byte[]
        // this order should not change LUA script depends on it
        private const string SetScript = @"
                redis.call('HMSET', KEYS[1], 'absexp', ARGV[1], 'sldexp', ARGV[2], 'data', ARGV[4])
                if ARGV[3] ~= '-1' then
                  redis.call('EXPIRE', KEYS[1], ARGV[3])
                end
                return 1";
        private const string AbsoluteExpirationKey = "absexp";
        private const string SlidingExpirationKey = "sldexp";
        private const string DataKey = "data";
        private const long NotPresent = -1;

        private static Lazy<ConnectionMultiplexer> _lazyConnection;
        internal IDatabase Database { get; set; }

        private readonly RedisCacheOptions _options;
        private readonly string _instance;

        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);

        public RedisCache(IOptions<RedisCacheOptions> optionsAccessor)
        {
            if (optionsAccessor == null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }

            _options = optionsAccessor.Value;

            // This allows partitioning a single backend cache for use with multiple apps/services.
            _instance = _options.InstanceName ?? string.Empty;
            Console.WriteLine("RedisCache created " + Guid.NewGuid().ToString());
        }

        public byte[] Get(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return GetAndRefresh(key, getData: true);
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            return await GetAndRefreshAsync(key, getData: true, token: token);
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            try
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (options == null)
                {
                    throw new ArgumentNullException(nameof(options));
                }

                Connect();

                var creationTime = DateTimeOffset.UtcNow;

                var absoluteExpiration = GetAbsoluteExpiration(creationTime, options);

                var result = Database.ScriptEvaluate(SetScript, new RedisKey[] { _instance + key },
                    new RedisValue[]
                    {
                        absoluteExpiration?.Ticks ?? NotPresent,
                        options.SlidingExpiration?.Ticks ?? NotPresent,
                        GetExpirationInSeconds(creationTime, absoluteExpiration, options) ?? NotPresent,
                        value
                    });
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while setting key {key}", ex);
            }
        }

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            token.ThrowIfCancellationRequested();

            this.Connect();

            var creationTime = DateTimeOffset.UtcNow;

            var absoluteExpiration = GetAbsoluteExpiration(creationTime, options);

            await Database.ScriptEvaluateAsync(SetScript, new RedisKey[] { _instance + key },
                new RedisValue[]
                {
                        absoluteExpiration?.Ticks ?? NotPresent,
                        options.SlidingExpiration?.Ticks ?? NotPresent,
                        GetExpirationInSeconds(creationTime, absoluteExpiration, options) ?? NotPresent,
                        value
                });
        }

        public void Refresh(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            GetAndRefresh(key, getData: false);
        }

        public async Task RefreshAsync(string key, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            await GetAndRefreshAsync(key, getData: false, token: token);
        }

        private void Connect()
        {
            if (this.Database != null)
                return;

            _connectionLock.Wait();
            try
            {
                if (_lazyConnection == null)
                {
                    _lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
                    {
                        if (_options.ConfigurationOptions != null)
                            return ConnectionMultiplexer.Connect(_options.ConfigurationOptions);
                        else
                            return ConnectionMultiplexer.Connect(_options.Configuration);
                    });
                }

                this.Database = _lazyConnection.Value.GetDatabase();
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        public List<string> GetKeys(string pattern = "*")
        {
            try
            {
                var endpoints = _lazyConnection.Value.GetEndPoints();
                var keys = new List<string>();
                foreach (var endpoint in _lazyConnection.Value.GetEndPoints())
                {
                    var server = _lazyConnection.Value.GetServer(endpoint);
                    if (!server.IsReplica) // sadece master node
                    {
                        keys.AddRange(server.Keys(pattern: pattern, pageSize: 1000).Where(op => !string.IsNullOrEmpty((string)op)).Select(op => (string)op)!.Cast<string>().ToList());
                    }
                }
                keys = keys.Distinct().ToList();
                return keys;
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        public Dictionary<string, List<string>> GetEndpointKeys(string pattern = "*")
        {
            try
            {
                var endpointKeys = new Dictionary<string, List<string>>();
                var endpoints = _lazyConnection.Value.GetEndPoints();
                foreach (var endpoint in _lazyConnection.Value.GetEndPoints())
                {
                    var keys = new List<string>();
                    var server = _lazyConnection.Value.GetServer(endpoint);
                    if (!server.IsReplica) // sadece master node
                    {
                        keys.AddRange(server.Keys(pattern: pattern, pageSize: 1000).Where(op => !string.IsNullOrEmpty((string)op)).Select(op => (string)op)!.Cast<string>().ToList());
                    }
                    endpointKeys[endpoint.ToString()] = keys;
                }
                return endpointKeys;
            }
            catch (Exception)
            {
                return new Dictionary<string, List<string>>();
            }
        }

        private byte[] GetAndRefresh(string key, bool getData)
        {
            try
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }
                _CacheKey = key;
                Connect();

                // This also resets the LRU status as desired.
                // TODO: Can this be done in one operation on the server side? Probably, the trick would just be the DateTimeOffset math.
                RedisValue[] results;
                RedisExtensions.GetCommandFlags = this._options.GetCommandFlags;
                if (getData)
                {
                    results = Database.HashMemberGet(_instance + key, AbsoluteExpirationKey, SlidingExpirationKey, DataKey);
                }
                else
                {
                    results = Database.HashMemberGet(_instance + key, AbsoluteExpirationKey, SlidingExpirationKey);
                }

                // TODO: Error handling
                if (results.Length >= 2)
                {
                    MapMetadata(results, out DateTimeOffset? absExpr, out TimeSpan? sldExpr);
                    Refresh(key, absExpr, sldExpr);
                }

                if (results.Length >= 3 && results[2].HasValue)
                {
                    return results[2];
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Exception while getting key {key}", ex);
            }
            return null;
        }

        private async Task<byte[]> GetAndRefreshAsync(string key, bool getData, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();
            _CacheKey = key;
            this.Connect();

            // This also resets the LRU status as desired.
            // TODO: Can this be done in one operation on the server side? Probably, the trick would just be the DateTimeOffset math.
            RedisValue[] results;
            RedisExtensions.GetCommandFlags = this._options.GetCommandFlags;
            if (getData)
            {
                results = await Database.HashMemberGetAsync(_instance + key, AbsoluteExpirationKey, SlidingExpirationKey, DataKey);
            }
            else
            {
                results = await Database.HashMemberGetAsync(_instance + key, AbsoluteExpirationKey, SlidingExpirationKey);
            }

            // TODO: Error handling
            if (results.Length >= 2)
            {
                MapMetadata(results, out DateTimeOffset? absExpr, out TimeSpan? sldExpr);
                await RefreshAsync(key, absExpr, sldExpr, token);
            }

            if (results.Length >= 3 && results[2].HasValue)
            {
                return results[2];
            }

            return null;
        }

        public void Remove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            _CacheKey = key;
            Connect();

            Database.KeyDelete(_instance + key);
            // TODO: Error handling
        }

        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            _CacheKey = key;
            this.Connect();

            await Database.KeyDeleteAsync(_instance + key);
            // TODO: Error handling
        }

        private void MapMetadata(RedisValue[] results, out DateTimeOffset? absoluteExpiration, out TimeSpan? slidingExpiration)
        {
            absoluteExpiration = null;
            slidingExpiration = null;
            var absoluteExpirationTicks = (long?)results[0];
            if (absoluteExpirationTicks.HasValue && absoluteExpirationTicks.Value != NotPresent)
            {
                absoluteExpiration = new DateTimeOffset(absoluteExpirationTicks.Value, TimeSpan.Zero);
            }
            var slidingExpirationTicks = (long?)results[1];
            if (slidingExpirationTicks.HasValue && slidingExpirationTicks.Value != NotPresent)
            {
                slidingExpiration = new TimeSpan(slidingExpirationTicks.Value);
            }
        }

        private void Refresh(string key, DateTimeOffset? absExpr, TimeSpan? sldExpr)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            _CacheKey = key;
            // Note Refresh has no effect if there is just an absolute expiration (or neither).
            TimeSpan? expr = null;
            if (sldExpr.HasValue)
            {
                if (absExpr.HasValue)
                {
                    var relExpr = absExpr.Value - DateTimeOffset.Now;
                    expr = relExpr <= sldExpr.Value ? relExpr : sldExpr;
                }
                else
                {
                    expr = sldExpr;
                }
                Database.KeyExpire(_instance + key, expr);
                // TODO: Error handling
            }
        }

        private async Task RefreshAsync(string key, DateTimeOffset? absExpr, TimeSpan? sldExpr, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            _CacheKey = key;
            token.ThrowIfCancellationRequested();

            // Note Refresh has no effect if there is just an absolute expiration (or neither).
            TimeSpan? expr = null;
            if (sldExpr.HasValue)
            {
                if (absExpr.HasValue)
                {
                    var relExpr = absExpr.Value - DateTimeOffset.Now;
                    expr = relExpr <= sldExpr.Value ? relExpr : sldExpr;
                }
                else
                {
                    expr = sldExpr;
                }
                await Database.KeyExpireAsync(_instance + key, expr);
                // TODO: Error handling
            }
        }

        private static long? GetExpirationInSeconds(DateTimeOffset creationTime, DateTimeOffset? absoluteExpiration, DistributedCacheEntryOptions options)
        {
            if (absoluteExpiration.HasValue && options.SlidingExpiration.HasValue)
            {
                return (long)Math.Min(
                    (absoluteExpiration.Value - creationTime).TotalSeconds,
                    options.SlidingExpiration.Value.TotalSeconds);
            }
            else if (absoluteExpiration.HasValue)
            {
                return (long)(absoluteExpiration.Value - creationTime).TotalSeconds;
            }
            else if (options.SlidingExpiration.HasValue)
            {
                return (long)options.SlidingExpiration.Value.TotalSeconds;
            }
            return null;
        }

        private static DateTimeOffset? GetAbsoluteExpiration(DateTimeOffset creationTime, DistributedCacheEntryOptions options)
        {
            if (options.AbsoluteExpiration.HasValue && options.AbsoluteExpiration <= creationTime)
                options.AbsoluteExpiration = Ophelia.Utility.Now.AddHours(1);

            var absoluteExpiration = options.AbsoluteExpiration;
            if (options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                absoluteExpiration = creationTime + options.AbsoluteExpirationRelativeToNow;
            }

            return absoluteExpiration;
        }

        public void Dispose()
        {
            this.Database = null;
        }
    }
}
