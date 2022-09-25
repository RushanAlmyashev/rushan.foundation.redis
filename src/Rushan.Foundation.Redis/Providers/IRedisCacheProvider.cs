using System;
using System.Threading.Tasks;

namespace Rushan.Foundation.Redis.Providers
{
    public interface IRedisCacheProvider : ICacheProvider
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

        /// <summary>
        /// Increments the number stored at key by increment. If the key does not exist,
        /// it is set to 0 before performing the operation. An error is returned if the key
        /// contains a value of the wrong type or contains a string that is not representable
        /// as integer. This operation is limited to 64 bit signed integers.
        /// </summary>
        /// <param name="cacheKey">The key of the string.</param>
        /// <param name="timeToLive">Time expirattion</param>
        void IncrementCounter(string counterKey, TimeSpan? timeToLive = null);


        /// <summary>
        /// Returns the remaining time to live of a key that has a timeout. This introspection
        /// capability allows a Redis client to check how many seconds a given key will continue
        /// to be part of the dataset.
        /// </summary>
        /// <param name="cacheKey">The key to check.</param>
        /// <returns></returns>
        TimeSpan? GetKeyExpirationTime(string cacheKey);

        /// <inheritdoc cref="AddOrUpdateValue{T}(string, T, TimeSpan)"/>
        Task AddOrUpdateValueAsync<T>(string cacheKey, T value, TimeSpan timeout);

        /// <inheritdoc cref="DeleteItem(string)"/>
        Task DeleteItemAsync(string cacheKey);

        /// <inheritdoc cref="IncrementCounter(string, TimeSpan?)"/>
        Task IncrementCounterAsync(string counterKey, TimeSpan? timeToLive = null);

        /// <inheritdoc cref="GetKeyExpirationTime(string)"/>
        Task<TimeSpan?> GetKeyExpirationTimeAsync(string cacheKey);
        int GetCounterValue(string counterKey);
        Task<int> GetCounterValueAsync(string counterKey);
    }
}