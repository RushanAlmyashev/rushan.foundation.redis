using System;
using System.Threading.Tasks;

namespace Rushan.Foundation.Redis.Persistences
{
    /// <summary>
    /// Ensure persistence connection to cache instence and provides base methods
    /// </summary>
    internal interface IRedisPersistence
    {
        /// <summary>
        /// Get the value of key. If the key does not exist - throw an exception.
        /// </summary>
        /// <param name="cacheKey">The key of the string.</param>
        /// <returns>The value of key, or nil when key does not exist.</returns>
        byte[] GetCachedValue(string cacheKey);

        /// <inheritdoc cref="GetCachedValue(string)"/>
        Task<byte[]> GetCachedValueAsync(string cacheKey);


        /// <summary>
        ///     Set key to hold the string value. If key already holds a value, it is overwritten,
        ///     regardless of its type.
        /// </summary>
        /// <param name="cacheKey">The key of the string</param>
        /// <param name="value">The value to set</param>
        /// <param name="timeout">The expiry to set</param>
        void SetCachedValue(string cacheKey, byte[] value, TimeSpan timeout);

        /// <inheritdoc cref="SetCachedValue(string, byte[], TimeSpan)"/>
        Task SetCachedValueAsync(string cacheKey, byte[] value, TimeSpan timeout);


        /// <summary>
        /// Removes the specified key. A key is ignored if it does not exist.
        /// </summary>
        /// <param name="cacheKey">The key to check.</param>
        void DeleteItem(string cacheKey);

        /// <inheritdoc cref="DeleteItem(string)"/>
        Task DeleteItemAsync(string cacheKey);


        /// <summary>
        /// Returns if key exists.
        /// </summary>
        /// <param name="cacheKey">The key to check.</param>
        /// <returns>true if the key exists. false if the key does not exist.</returns>
        bool ContainsKey(string cacheKey);

        /// <inheritdoc cref="ContainsKey(string)"/>
        Task<bool> ContainsKeyAsync(string cacheKey);


        /// <summary>
        /// Set a timeout on key. After the timeout has expired, the key will automatically
        //  be deleted. A key with an associated timeout is said to be volatile in Redis
        //  terminology.
        /// </summary>
        /// <param name="cacheKey">The key to set the expiration for.</param>
        /// <param name="timeout">The timeout to set.</param>
        void SetCacheKeyExpire(string cacheKey, TimeSpan timeout);

        /// <inheritdoc cref="SetCacheKeyExpire(string, TimeSpan)"/>
        Task SetCacheKeyExpireAsync(string cacheKey, TimeSpan timeout);


        /// <summary>
        /// Takes a lock (specifying a token value) if it is not already taken.
        /// </summary>
        /// <param name="lockKey">The key of the lock.</param>
        /// <param name="lockValue">The value to set at the key.</param>
        /// <param name="expiry">The expiration of the lock key.</param>
        /// <returns>true if the lock was successfully taken, false otherwise.</returns>
        bool LockTake(string lockKey, string lockValue, TimeSpan expiry);


        /// <summary>
        /// Releases a lock, if the token value is correct.
        /// </summary>
        /// <param name="lockKey">The key of the lock.</param>
        /// <param name="lockValue">The value at the key that must match.</param>
        void LockRelease(string lockKey, string lockValue);


        /// <summary>
        /// Returns the remaining time to live of a key that has a timeout. This introspection
        /// capability allows a Redis client to check how many seconds a given key will continue
        /// to be part of the dataset.
        /// </summary>
        /// <param name="cacheKey">The key to check.</param>
        /// <returns>TTL, or nil when key does not exist or does not have a timeout.</returns>
        TimeSpan? KeyTimeToLive(string cacheKey);

        /// <inheritdoc cref="KeyTimeToLive(string)"/>
        Task<TimeSpan?> KeyTimeToLiveAsync(string cacheKey);

        /// <summary>
        /// Increments the number stored at key by increment. If the key does not exist,
        /// it is set to 0 before performing the operation. An error is returned if the key
        /// contains a value of the wrong type or contains a string that is not representable
        /// as integer. This operation is limited to 64 bit signed integers.
        /// </summary>
        /// <param name="cacheKey">The key of the string.</param>
        void StringIncrement(string cacheKey);

        /// <inheritdoc cref="StringIncrement(string)"/>
        Task StringIncrementAsync(string cacheKey);
    }
}
