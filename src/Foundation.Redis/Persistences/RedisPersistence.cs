using Foundation.Redis.Logger;
using StackExchange.Redis;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Foundation.Redis.Persistences
{
    internal class RedisPersistence : IRedisPersistence
    {            
        private readonly ILogger _logger;

        private readonly TimeSpan _reconnectionTimeout = TimeSpan.FromSeconds(5);
        private readonly object lockObject = new object();

        private readonly Lazy<ConnectionMultiplexer> _lazyConnection;
        private ConnectionMultiplexer _redis => _lazyConnection.Value;

        /// <summary>
        /// Default class constructor
        /// </summary>
        /// <param name="connectionString">The configuration string for redis instance</param>
        /// <param name="logger">logger</param>
        internal RedisPersistence(string connectionString, ILogger logger)
        {
            _logger = logger;
            _lazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectToRedisCluster(connectionString));
        }
               

        /// <inheritdoc/>
        public byte[] GetCachedValue(string cacheKey)
        {
            var db = _redis.GetDatabase();

            var data = db.StringGet(cacheKey);

            if (string.IsNullOrEmpty(data))
            {
                throw new Exception("data from db is empty");
            }

            return data;
        }

        /// <inheritdoc/>
        public async Task<byte[]> GetCachedValueAsync(string cacheKey)
        {
            var db = _redis.GetDatabase();

            var data = await db.StringGetAsync(cacheKey);

            if (string.IsNullOrEmpty(data))
            {
                throw new Exception("data from db is empty");
            }

            return data;
        }

        /// <inheritdoc/>
        public void SetCachedValue(string cacheKey, byte[] value, TimeSpan timeout)
        {
            var db = _redis.GetDatabase();

            db.StringSet(cacheKey, value, timeout);
        }

        /// <inheritdoc/>
        public async Task SetCachedValueAsync(string cacheKey, byte[] value, TimeSpan timeout)
        {
            var db = _redis.GetDatabase();

            await db.StringSetAsync(cacheKey, value, timeout);
        }

        /// <inheritdoc/>
        public bool ContainsKey(string cacheKey)
        {
            try
            {
                var db = _redis.GetDatabase();
                    
                return db.KeyExists(cacheKey);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"can't check contains of key {cacheKey}");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ContainsKeyAsync(string cacheKey)
        {
            try
            {
                var db = _redis.GetDatabase();

                return await db.KeyExistsAsync(cacheKey);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"can't check contains of key {cacheKey}");
                return false;
            }
        }

        /// <inheritdoc/>
        public void DeleteItem(string cacheKey)
        {
            try
            {
                var db = _redis.GetDatabase();
                
                db.KeyDelete(cacheKey);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"can't remove key {cacheKey}");
            }
        }

        /// <inheritdoc/>
        public async Task DeleteItemAsync(string cacheKey)
        {
            try
            {
                var db = _redis.GetDatabase();
                await db.KeyDeleteAsync(cacheKey);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"can't remove key {cacheKey}");
            }
        }

        /// <inheritdoc/>
        public void SetCacheKeyExpire(string cacheKey, TimeSpan timeout)
        {
            try
            {
                var db = _redis.GetDatabase();
                db.KeyExpire(cacheKey, timeout);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"can't set key expire {cacheKey}");
            }
        }

        /// <inheritdoc/>
        public async Task SetCacheKeyExpireAsync(string cacheKey, TimeSpan timeout)
        {
            try
            {
                var db = _redis.GetDatabase();
                await db.KeyExpireAsync(cacheKey, timeout);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"can't set key expire {cacheKey}");
            }
        }

        /// <inheritdoc/>
        public bool LockTake(string lockKey, string lockValue, TimeSpan expiry)
        {
            try
            {
                var db = _redis.GetDatabase();
                    
                return db.LockTake(lockKey, lockValue,  expiry);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"error on lock take {lockKey}");

                return false;
            }
        }

        /// <inheritdoc/>
        public void LockRelease(string lockKey, string lockValue)
        {
            try
            {
                var db = _redis.GetDatabase();
                db.LockRelease(lockKey, lockValue);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"error on lock release {lockKey}");
            }
        }


        /// <inheritdoc/>
        public TimeSpan? KeyTimeToLive(string cacheKey)
        {
            try
            {
                var db = _redis.GetDatabase();
                return db.KeyTimeToLive(cacheKey);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"error on get key time to live {cacheKey}");
            }

            return null;
        }

        /// <inheritdoc/>
        public async Task<TimeSpan?> KeyTimeToLiveAsync(string cacheKey)
        {
            try
            {
                var db = _redis.GetDatabase();                
                return await db.KeyTimeToLiveAsync(cacheKey);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"error on get key time to live {cacheKey}");
            }

            return null;
        }


        /// <summary>
        /// Connect to redis cluster with ExponentialRetry on connection failed
        /// </summary>
        /// <param name="connectionString">The configuration string for redis instance</param>
        /// <returns></returns>
        private ConnectionMultiplexer ConnectToRedisCluster(string connectionString)
        {
            lock (lockObject)
            {
                if (_lazyConnection.IsValueCreated && _redis.IsConnected)
                {
                    _logger.Warn("Redis connection is already open");
                    
                    return _redis;
                }

                try
                {
                    connectionString = Environment.ExpandEnvironmentVariables(connectionString);

                    _logger.Info($"Initializing connection to redis cluster {connectionString}");

                    var configuration = ConfigurationOptions.Parse(connectionString);
                    var deltaBackOffMilliseconds = (int)_reconnectionTimeout.TotalMilliseconds;

                    configuration.ReconnectRetryPolicy = new ExponentialRetry(deltaBackOffMilliseconds);
                    var redis = ConnectionMultiplexer.Connect(configuration);

                    redis.ConnectionFailed += (sender, args) =>
                    {
                        _logger.Info($"Connection failed to {args.EndPoint}");

                        LogStatus((ConnectionMultiplexer)sender);
                    };

                    redis.ConnectionRestored += (sender, args) =>
                    {
                        _logger.Info("Connection restored to {args.EndPoint}");
                        LogStatus((ConnectionMultiplexer)sender);
                    };

                    _logger.Info($"Connected to redis with configuration {connectionString}");

                    return redis;
                }
                catch (Exception e)
                {
                    _logger.Error(e, $"can't connect to redis");

                    throw;
                }
            }
        }       

        /// <summary>
        /// Logger for connection status on Failed/Restore
        /// </summary>
        /// <param name="redis"></param>
        private void LogStatus(ConnectionMultiplexer redis)
        {
            try
            {
                var endPoints = redis.GetEndPoints();

                foreach (var endPoint in endPoints)
                {
                    var dnsEndPoint = (DnsEndPoint)endPoint;
                    var server = redis.GetServer(dnsEndPoint.Host, dnsEndPoint.Port);                    
                    _logger.Info($"server status: Host:{dnsEndPoint.Host}, port:{dnsEndPoint.Port}, isConnected:{server.IsConnected}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"can't check connections to redis: {redis.Configuration}.");
            }
        }
    }
}
