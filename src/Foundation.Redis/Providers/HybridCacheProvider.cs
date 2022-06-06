using Foundation.Redis.Configuration;
using Foundation.Redis.Helpers;
using Foundation.Redis.Logger;
using Foundation.Redis.Persistences;
using Foundation.Redis.Serialization;
using System;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace Foundation.Redis.Providers
{
    /// <summary>
    /// HybridCacheProvider - ensure two level cache.
    /// NB! Do not use for distributed systems.
    /// 1 level: Manage runtime cache. Attempt to get value from memory
    /// 2 level: Manage redis cache. Attempt to get value from redis instance and fullfill memory cache
    /// If both cache are empty - get value from source
    /// </summary>
    public class HybridCacheProvider : BaseCacheProvider, ICacheProvider
    {        
        private readonly IRedisPersistence _redisPersistence;
        private static readonly MemoryCache _memoryCache = MemoryCache.Default;

        private readonly TimeSpan _defaultCacheTime;
        private readonly ILogger _logger;
        private readonly ISerializer _serializer;                

        /// <summary>
        /// Initializes a new instance of the <see cref="HybridCacheProvider"/> class.
        /// </summary>       
        public HybridCacheProvider(RedisConfigurationSection settings,
            ILogger logger = null,
            ISerializer serializer = null)
        {
            _serializer = serializer ?? new JsonSerializer();
            _logger = logger ?? new EmptyLogger();
            _defaultCacheTime = settings.DefaultCacheTime ?? TimeSpan.FromHours(1);

            _redisPersistence = new RedisPersistence(settings.ConnectionString, _logger);
        }

        /// <summary>
        /// Contructor for a new instance of the RedisCacheProvider <see cref="HybridCacheProvider"/> class.
        /// </summary>
        /// <param name="connectionString">string for connection to redis</param>
        /// <param name="defaultCacheTime">default cache time</param>
        /// <param name="logger">The logger</param>
        /// <param name="serializer">data serializer</param>
        public HybridCacheProvider(string connectionString,
            TimeSpan? defaultCacheTime = null,
            ILogger logger = null,
            ISerializer serializer = null)
        {
            _logger = logger ?? new EmptyLogger();
            _serializer = serializer ?? new JsonSerializer();
            _defaultCacheTime = defaultCacheTime ?? TimeSpan.FromHours(1);

            _redisPersistence = new RedisPersistence(connectionString, _logger);
        }

        internal HybridCacheProvider(IRedisPersistence redisPersistence,
            ILogger logger = null,
            ISerializer serializer = null)
        {
            _redisPersistence = redisPersistence;
            _defaultCacheTime = TimeSpan.FromHours(1);
            
            _logger = logger ?? new EmptyLogger();
            _serializer = serializer ?? new JsonSerializer();
        }

        /// <inheritdoc />        
        public T GetOrAddFromCache<T>(Func<T> execFunc, string cacheKey) 
            => GetOrAddFromCache(execFunc, cacheKey, _defaultCacheTime);

        public async Task<T> GetOrAddFromCacheAsync<T>(Func<Task<T>> execFuncAsync, string cacheKey)
            => await GetOrAddFromCacheAsync(execFuncAsync, cacheKey, _defaultCacheTime);

        public async Task<T> GetOrAddFromCacheAsync<T>(Func<T> execFunc, string cacheKey)
        => await GetOrAddFromCacheAsync(execFunc, cacheKey, _defaultCacheTime);


        /// <inheritdoc />
        public T GetOrAddFromCache<T>(Func<T> execFunc, string cacheKey, TimeSpan cacheTime)
        {
            ValidateKey(cacheKey);

            if (_memoryCache.Contains(cacheKey))
            {
                return MemoryCacheGet<T>(cacheKey);
            }

            T result;
            TimeSpan? redisCacheTime;

            if (_redisPersistence.ContainsKey(cacheKey))
            {
                var byteValue = _redisPersistence.GetCachedValue(cacheKey);
                result = _serializer.Deserialize<T>(byteValue);

                redisCacheTime = _redisPersistence.KeyTimeToLive(cacheKey);
            }
            else
            {
                result = execFunc();
                redisCacheTime = cacheTime;

                var byteResult = _serializer.Serialize<T>(result);
                _redisPersistence.SetCachedValue(cacheKey, byteResult, cacheTime);
            }

            var cacheItemPolicy = CacheItemPolicyHelper.ComputeCacheItemPolicy(redisCacheTime);
            MemoryCacheSet(cacheKey, result, cacheItemPolicy);           

            return result;            
        }

        /// <inheritdoc />
        public async Task<T> GetOrAddFromCacheAsync<T>(Func<Task<T>> execFuncAsync, string cacheKey, TimeSpan cacheTime)
        {
            ValidateKey(cacheKey);

            if (_memoryCache.Contains(cacheKey))
            {
                return MemoryCacheGet<T>(cacheKey);
            }

            T result;
            TimeSpan? redisCacheTime;

            if (await _redisPersistence.ContainsKeyAsync(cacheKey))
            {
                var byteValue = await _redisPersistence.GetCachedValueAsync(cacheKey);
                result = _serializer.Deserialize<T>(byteValue);

                redisCacheTime = await _redisPersistence.KeyTimeToLiveAsync(cacheKey);
            }
            else
            {
                result = await execFuncAsync();                

                var byteResult = _serializer.Serialize<T>(result);
                await _redisPersistence.SetCachedValueAsync(cacheKey, byteResult, cacheTime);
                
                redisCacheTime = cacheTime;
            }

            var cacheItemPolicy = CacheItemPolicyHelper.ComputeCacheItemPolicy(redisCacheTime);
            MemoryCacheSet(cacheKey, result, cacheItemPolicy);

            return result;
        }

        /// <inheritdoc />
        public async Task<T> GetOrAddFromCacheAsync<T>(Func<T> execFunc, string cacheKey, TimeSpan cacheTime)
        {
            ValidateKey(cacheKey);

            if (_memoryCache.Contains(cacheKey))
            {
                return MemoryCacheGet<T>(cacheKey);
            }

            T result;
            TimeSpan? redisCacheTime;

            if (await _redisPersistence.ContainsKeyAsync(cacheKey))
            {
                var byteValue = await _redisPersistence.GetCachedValueAsync(cacheKey);
                result = _serializer.Deserialize<T>(byteValue);

                redisCacheTime = await _redisPersistence.KeyTimeToLiveAsync(cacheKey);
            }
            else
            {
                result = execFunc();

                var byteResult = _serializer.Serialize<T>(result);
                await _redisPersistence.SetCachedValueAsync(cacheKey, byteResult, cacheTime);

                redisCacheTime = cacheTime;
            }

            var cacheItemPolicy = CacheItemPolicyHelper.ComputeCacheItemPolicy(redisCacheTime);
            MemoryCacheSet(cacheKey, result, cacheItemPolicy);

            return result;
        }


        /// <inheritdoc />
        public bool TryGet<T>(string cacheKey, out T value)
        {
            ValidateKey(cacheKey);

            if (_memoryCache.Contains(cacheKey))
            {
                value = MemoryCacheGet<T>(cacheKey);
                
                return true;
            }


            if (_redisPersistence.ContainsKey(cacheKey))
            {
                var byteValue = _redisPersistence.GetCachedValue(cacheKey);
                value = _serializer.Deserialize<T>(byteValue);

                var keyTimeToLive = _redisPersistence.KeyTimeToLive(cacheKey);
                var cacheItemPolicy = CacheItemPolicyHelper.ComputeCacheItemPolicy(keyTimeToLive);

                MemoryCacheSet(cacheKey, value, cacheItemPolicy);

                return true;
            }

            value = default;
            return false;
        }


        /// <inheritdoc />
        public T GetCachedValue<T>(string cacheKey)
        {
            ValidateKey(cacheKey);

            if (_memoryCache.Contains(cacheKey))
            {
                return MemoryCacheGet<T>(cacheKey);
            }

            var stringValue = _redisPersistence.GetCachedValue(cacheKey);
            var value = _serializer.Deserialize<T>(stringValue);

            var keyTimeToLive = _redisPersistence.KeyTimeToLive(cacheKey);
            var cacheItemPolicy = CacheItemPolicyHelper.ComputeCacheItemPolicy(keyTimeToLive);

            MemoryCacheSet(cacheKey, value, cacheItemPolicy);

            return value;
        }

        /// <inheritdoc />
        public async Task<T> GetCachedValueAsync<T>(string cacheKey)
        {
            ValidateKey(cacheKey);

            if (_memoryCache.Contains(cacheKey))
            {
                return MemoryCacheGet<T>(cacheKey);
            }

            var stringValue = await _redisPersistence.GetCachedValueAsync(cacheKey);
            var value = _serializer.Deserialize<T>(stringValue);

            var keyTimeToLive = await _redisPersistence.KeyTimeToLiveAsync(cacheKey);
            var cacheItemPolicy = CacheItemPolicyHelper.ComputeCacheItemPolicy(keyTimeToLive);

            MemoryCacheSet(cacheKey, value, cacheItemPolicy);

            return value;
        }


        /// <inheritdoc />
        public bool ContainsKey(string cacheKey)
        {
            ValidateKey(cacheKey);

            if (_memoryCache.Contains(cacheKey))
            {
                return true;
            }

            if (_redisPersistence.ContainsKey(cacheKey))
            {
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public async Task<bool> ContainsKeyAsync(string cacheKey)
        {
            ValidateKey(cacheKey);

            if (_memoryCache.Contains(cacheKey))
            {
                return true;
            }

            if (await _redisPersistence.ContainsKeyAsync(cacheKey))
            {
                return true;
            }

            return false;
        }

        private static void MemoryCacheSet<T>(string cacheKey, T value, CacheItemPolicy policy)
        {
            if (value == null)
                _memoryCache.Set(cacheKey, DBNull.Value, policy);
            else
                _memoryCache.Set(cacheKey, value, policy);
        }        

        private static T MemoryCacheGet<T>(string cacheKey)
        {
            object cachedValue = _memoryCache.Get(cacheKey);

            if (cachedValue.Equals(DBNull.Value))
            {
                return default(T);
            }

            return (T)cachedValue;
        }
    }
}
