using System;
using System.Threading.Tasks;
using Rushan.Foundation.Redis.Serialization;
using Rushan.Foundation.Redis.Persistences;
using Rushan.Foundation.Redis.Logger;
using Rushan.Foundation.Redis.Configuration;

namespace Rushan.Foundation.Redis.Providers.Impl
{
    /// <summary>
    /// Manage redis cache instance
    /// </summary>
    public class RedisCacheProvider : BaseCacheProvider, ICacheProvider, IRedisCacheProvider
    {
        private readonly IRedisPersistence _redisPersistence;

        private readonly TimeSpan _defaultCacheTime;
        private readonly ISerializer _serializer;
        private readonly ILogger _logger;

        /// <summary>
        /// Contructor for a new instance of the RedisCacheProvider <see cref="RedisCacheProvider"/> class.
        /// </summary>
        /// <param name="redisConfigurationSection">Redis configuration</param>
        /// <param name="logger">Logger</param>
        /// <param name="serializer">Serializer</param>
        public RedisCacheProvider(RedisConfigurationSection settings,
            ILogger logger = null,
            ISerializer serializer = null)
        {
            _logger = logger ?? new EmptyLogger();
            _serializer = serializer ?? new JsonSerializer();
            _defaultCacheTime = settings.DefaultCacheTime ?? TimeSpan.FromHours(1);

            _redisPersistence = new RedisPersistence(settings.ConnectionString, _logger);
        }

        /// <summary>
        /// Contructor for a new instance of the RedisCacheProvider <see cref="RedisCacheProvider"/> class.
        /// </summary>
        /// <param name="connectionString">string for connection to redis</param>
        /// <param name="defaultCacheTime">default cache time</param>
        /// <param name="logger">The logger</param>
        /// <param name="serializer">data serializer</param>
        public RedisCacheProvider(string connectionString,
            TimeSpan? defaultCacheTime = null,
            ILogger logger = null,
            ISerializer serializer = null)
        {
            _logger = logger ?? new EmptyLogger();
            _serializer = serializer ?? new JsonSerializer();
            _defaultCacheTime = defaultCacheTime ?? TimeSpan.FromHours(1);

            _redisPersistence = new RedisPersistence(connectionString, _logger);
        }


        internal RedisCacheProvider(IRedisPersistence redisPersistence,
            ILogger logger = null,
            ISerializer serializer = null)
        {
            _redisPersistence = redisPersistence;

            _logger = logger ?? new EmptyLogger();
            _serializer = serializer ?? new JsonSerializer();
            _defaultCacheTime = TimeSpan.FromHours(1);
        }

        /// <inheritdoc/>
        public T GetOrAddFromCache<T>(Func<T> execFunc, string cacheKey) => GetOrAddFromCache(execFunc, cacheKey, _defaultCacheTime);

        public async Task<T> GetOrAddFromCacheAsync<T>(Func<Task<T>> execFunc, string cacheKey) => await GetOrAddFromCacheAsync(execFunc, cacheKey, _defaultCacheTime);

        public async Task<T> GetOrAddFromCacheAsync<T>(Func<T> execFunc, string cacheKey) => await GetOrAddFromCacheAsync(execFunc, cacheKey, _defaultCacheTime);

        /// <inheritdoc/>
        public T GetOrAddFromCache<T>(Func<T> execFunc, string cacheKey, TimeSpan cacheTime)
        {
            ValidateKey(cacheKey);

            try
            {
                if (_redisPersistence.ContainsKey(cacheKey))
                {
                    return GetCachedValue<T>(cacheKey);
                }

                var value = execFunc();
                var cachedValue = _serializer.Serialize(value);

                _redisPersistence.SetCachedValue(cacheKey, cachedValue, cacheTime);

                return value;
            }
            catch (Exception e)
            {
                _logger.Error(e, $"can't add or update value for {cacheKey}");
                return default;
            }
        }

        public async Task<T> GetOrAddFromCacheAsync<T>(Func<Task<T>> execFuncAsync, string cacheKey, TimeSpan cacheTime)
        {
            ValidateKey(cacheKey);

            try
            {
                var isContainsKey = await _redisPersistence.ContainsKeyAsync(cacheKey);
                if (isContainsKey)
                {
                    return await GetCachedValueAsync<T>(cacheKey);
                }

                var value = await execFuncAsync();
                var cachedValue = _serializer.Serialize(value);

                await _redisPersistence.SetCachedValueAsync(cacheKey, cachedValue, cacheTime);

                return value;
            }
            catch (Exception e)
            {
                _logger.Error(e, $"can't add or update value for {cacheKey}");
                return default;
            }
        }

        public async Task<T> GetOrAddFromCacheAsync<T>(Func<T> execFunc, string cacheKey, TimeSpan cacheTime)
        {
            ValidateKey(cacheKey);

            try
            {
                var isContainsKey = await _redisPersistence.ContainsKeyAsync(cacheKey);
                if (isContainsKey)
                {
                    return await GetCachedValueAsync<T>(cacheKey);
                }

                var value = execFunc();
                var cachedValue = _serializer.Serialize(value);

                await _redisPersistence.SetCachedValueAsync(cacheKey, cachedValue, cacheTime);

                return value;
            }
            catch (Exception e)
            {
                _logger.Error(e, $"can't add or update value for {cacheKey}");
                return default;
            }
        }

        /// <inheritdoc/>
        public void AddOrUpdateValue<T>(string cacheKey, T value, TimeSpan cacheTime)
        {
            ValidateKey(cacheKey);

            var stringValue = _serializer.Serialize(value);
            _redisPersistence.SetCachedValue(cacheKey, stringValue, cacheTime);
        }

        /// <inheritdoc/>
        public async Task AddOrUpdateValueAsync<T>(string cacheKey, T value, TimeSpan cacheTime)
        {
            ValidateKey(cacheKey);

            var stringValue = _serializer.Serialize(value);
            await _redisPersistence.SetCachedValueAsync(cacheKey, stringValue, cacheTime);
        }


        public T GetCachedValue<T>(string cacheKey)
        {
            ValidateKey(cacheKey);

            var cachedValue = _redisPersistence.GetCachedValue(cacheKey);

            if (cachedValue == null)
            {
                throw new Exception("Cached value from db is empty");
            }

            var value = _serializer.Deserialize<T>(cachedValue);
            return value;
        }


        public async Task<T> GetCachedValueAsync<T>(string cacheKey)
        {
            ValidateKey(cacheKey);

            var cachedValue = await _redisPersistence.GetCachedValueAsync(cacheKey);

            if (cachedValue == null)
            {
                throw new Exception("Cached value from db is empty");
            }

            var value = _serializer.Deserialize<T>(cachedValue);
            return value;
        }


        /// <inheritdoc/>
        public bool TryGet<T>(string cacheKey, out T value)
        {
            ValidateKey(cacheKey);

            if (_redisPersistence.ContainsKey(cacheKey))
            {
                value = GetCachedValue<T>(cacheKey);
                return true;
            }

            value = default;
            return false;
        }

        /// <inheritdoc/>
        public bool ContainsKey(string cacheKey)
        {
            ValidateKey(cacheKey);

            try
            {
                return _redisPersistence.ContainsKey(cacheKey);
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
            ValidateKey(cacheKey);

            try
            {
                return await _redisPersistence.ContainsKeyAsync(cacheKey);
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
            ValidateKey(cacheKey);

            try
            {
                _redisPersistence.DeleteItem(cacheKey);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"can't delete key {cacheKey}");
            }
        }

        /// <inheritdoc/>
        public async Task DeleteItemAsync(string cacheKey)
        {
            ValidateKey(cacheKey);

            try
            {
                await _redisPersistence.DeleteItemAsync(cacheKey);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"can't delete key {cacheKey}");
            }
        }

        /// <inheritdoc/>
        public void IncrementCounter(string counterKey, TimeSpan? timeToLive = null)
        {
            _redisPersistence.StringIncrement(counterKey);

            if (timeToLive != null)
            {
                _redisPersistence.SetCacheKeyExpire(counterKey, timeToLive.Value);
            }
        }

        /// <inheritdoc/>
        public async Task IncrementCounterAsync(string counterKey, TimeSpan? timeToLive = null)
        {
            await _redisPersistence.StringIncrementAsync(counterKey);

            if (timeToLive != null)
            {
                await _redisPersistence.SetCacheKeyExpireAsync(counterKey, timeToLive.Value);
            }
        }

        public int GetCounterValue(string counterKey)
        {
            var byteArrayValue = _redisPersistence.GetCachedValue(counterKey);

            var value = Convert.ToInt32(System.Text.Encoding.UTF8.GetString(byteArrayValue));

            return value;
        }

        public async Task<int> GetCounterValueAsync(string counterKey)
        {
            var byteArrayValue = await _redisPersistence.GetCachedValueAsync(counterKey);

            var value = Convert.ToInt32(System.Text.Encoding.UTF8.GetString(byteArrayValue));

            return value;
        }

        /// <inheritdoc/>
        public TimeSpan? GetKeyExpirationTime(string cacheKey)
        {
            return _redisPersistence.KeyTimeToLive(cacheKey);
        }

        /// <inheritdoc/>
        public async Task<TimeSpan?> GetKeyExpirationTimeAsync(string cacheKey)
        {
            return await _redisPersistence.KeyTimeToLiveAsync(cacheKey);
        }
    }
}
