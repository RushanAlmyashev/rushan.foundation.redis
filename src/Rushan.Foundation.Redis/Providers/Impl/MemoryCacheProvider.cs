using System;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace Rushan.Foundation.Redis.Providers.Impl
{
    public class MemoryCacheProvider : BaseCacheProvider, ICacheProvider
    {
        private static readonly MemoryCache _memoryCache = MemoryCache.Default;
        private readonly CacheItemPolicy _defaultCachePolicy;

        public MemoryCacheProvider(TimeSpan defaultCacheTime)
        {
            _defaultCachePolicy = new CacheItemPolicy
            {
                SlidingExpiration = defaultCacheTime
            };
        }

        public T GetOrAddFromCache<T>(Func<T> execFunc, string cacheKey)
            => GetOrAddFromCache(execFunc, cacheKey, _defaultCachePolicy);

        public async Task<T> GetOrAddFromCacheAsync<T>(Func<Task<T>> execFuncAsync, string cacheKey)
            => await GetOrAddFromCacheAsync(execFuncAsync, cacheKey, _defaultCachePolicy);

#pragma warning disable 1998
        public async Task<T> GetOrAddFromCacheAsync<T>(Func<T> execFunc, string cacheKey)
            => GetOrAddFromCache(execFunc, cacheKey, _defaultCachePolicy);
#pragma warning restore 1998


        public T GetOrAddFromCache<T>(Func<T> execFunc, string cacheKey, TimeSpan timeout)
        {
            var dateTimeOffset = DateTimeOffset.UtcNow.Add(timeout);

            var cachePolicy = new CacheItemPolicy
            {
                AbsoluteExpiration = dateTimeOffset
            };

            return GetOrAddFromCache(execFunc, cacheKey, cachePolicy);
        }

        public async Task<T> GetOrAddFromCacheAsync<T>(Func<Task<T>> execFuncAsync, string cacheKey, TimeSpan timeout)
        {
            var dateTimeOffset = DateTimeOffset.UtcNow.Add(timeout);

            var cachePolicy = new CacheItemPolicy
            {
                AbsoluteExpiration = dateTimeOffset
            };

            return await GetOrAddFromCacheAsync(execFuncAsync, cacheKey, cachePolicy);
        }

#pragma warning disable 1998
        public async Task<T> GetOrAddFromCacheAsync<T>(Func<T> execFunc, string cacheKey, TimeSpan timeout)
            => GetOrAddFromCache(execFunc, cacheKey, timeout);
#pragma warning restore 1998


        /// <inheritdoc />
        private T GetOrAddFromCache<T>(Func<T> execFunc, string cacheKey, CacheItemPolicy cacheItemPolicy)
        {
            ValidateKey(cacheKey);

            if (_memoryCache.Contains(cacheKey))
            {
                return MemoryCacheGet<T>(cacheKey);
            }

            var result = execFunc();

            MemoryCacheSet(cacheKey, result, cacheItemPolicy);

            return result;
        }

        /// <inheritdoc />
        private async Task<T> GetOrAddFromCacheAsync<T>(Func<Task<T>> execFuncAsync, string cacheKey, CacheItemPolicy cacheItemPolicy)
        {
            ValidateKey(cacheKey);

            if (_memoryCache.Contains(cacheKey))
            {
                return MemoryCacheGet<T>(cacheKey);
            }

            var result = await execFuncAsync();

            MemoryCacheSet(cacheKey, result, cacheItemPolicy);

            return result;
        }

        public bool TryGet<T>(string cacheKey, out T value)
        {
            ValidateKey(cacheKey);

            if (_memoryCache.Contains(cacheKey))
            {
                value = MemoryCacheGet<T>(cacheKey);

                return true;
            }

            value = default;
            return false;
        }


        public T GetCachedValue<T>(string cacheKey)
        {
            ValidateKey(cacheKey);

            if (_memoryCache.Contains(cacheKey))
            {
                return MemoryCacheGet<T>(cacheKey);
            }

            throw new InvalidOperationException("Oops something went wrong");
        }

        /// <inheritdoc />
#pragma warning disable 1998
        public async Task<T> GetCachedValueAsync<T>(string cacheKey)
            => GetCachedValue<T>(cacheKey);
#pragma warning restore 1998



        public bool ContainsKey(string cacheKey)
        {
            ValidateKey(cacheKey);

            if (_memoryCache.Contains(cacheKey))
            {
                return true;
            }

            return false;
        }

#pragma warning disable 1998
        public async Task<bool> ContainsKeyAsync(string cacheKey) => ContainsKey(cacheKey);
#pragma warning restore 1998

        private static T MemoryCacheGet<T>(string cacheKey)
        {
            object cachedValue = _memoryCache.Get(cacheKey);

            if (cachedValue.Equals(DBNull.Value))
            {
                return default;
            }

            return (T)cachedValue;
        }

        private static void MemoryCacheSet<T>(string cacheKey, T value, CacheItemPolicy policy)
        {
            if (value == null)
                _memoryCache.Set(cacheKey, DBNull.Value, policy);
            else
                _memoryCache.Set(cacheKey, value, policy);
        }
    }

}
