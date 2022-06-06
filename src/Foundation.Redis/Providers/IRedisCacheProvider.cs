using System;
using System.Threading.Tasks;

namespace Foundation.Redis
{
    public interface IRedisCacheProvider: ICacheProvider
    {
        /// <summary>
        /// Inserts or update existed cache entry into the cache using the specified key and value with time expiration
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value</param>
        /// <param name="cacheKey">The cache key</param>
        /// <param name="timeout">Time expirattion</param>
        void AddOrUpdateValue<T>(string cacheKey, T value, TimeSpan timeout);

        /// <summary>
        /// Remove cache value by key
        /// </summary>
        /// <param name="cacheKey">The cache key.</param>
        void DeleteItem(string cacheKey);

        /// <inheritdoc cref="AddOrUpdateValue{T}(string, T, TimeSpan)"/>
        Task AddOrUpdateValueAsync<T>(string cacheKey, T value, TimeSpan timeout);

        /// <inheritdoc cref="DeleteItem(string)"/>
        Task DeleteItemAsync(string cacheKey);
    }
}