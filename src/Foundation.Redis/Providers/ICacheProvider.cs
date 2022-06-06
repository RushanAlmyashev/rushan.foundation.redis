﻿using System;
using System.Threading.Tasks;

namespace Foundation.Redis
{
    /// <summary>
    /// Interface for manage cache
    /// </summary>
    public interface ICacheProvider
    {        
        /// <summary>
        /// Get value from cache, otherwise execute func with put result into cache
        /// </summary>
        /// <typeparam name="T">Generic object</typeparam>
        /// <param name="execFunc">The execute function.</param>
        /// <param name="cacheKey">The cache key.</param>
        /// <returns></returns>
        T GetOrAddFromCache<T>(Func<T> execFunc, string cacheKey);        

        /// <summary>
        /// Get value from cache, otherwise execute func with put result into cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="execFunc">The execute function.</param>
        /// <param name="cacheKey">The cache key.</param>
        /// <param name="timeout">Cache item expiration time.</param>
        /// <returns>Generic object</returns>
        /// <autogeneratedoc />
        T GetOrAddFromCache<T>(Func<T> execFunc, string cacheKey, TimeSpan timeout);        

        /// <summary>
        /// Get value from cache by key
        /// </summary>
        /// <typeparam name="T">Generic object</typeparam>
        /// <param name="cacheKey">The cache key.</param>
        /// <returns></returns>
        T GetCachedValue<T>(string cacheKey);
               
        /// <summary>
        /// Check is key contains in cache
        /// </summary>
        /// <param name="cacheKey">The cache key.</param>
        /// <returns>
        ///   <c>true</c> if the specified cache key contains key; otherwise, <c>false</c>.
        /// </returns>
        bool ContainsKey(string cacheKey);        
              
                
        /// <summary>
        /// Try get cache value by key. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey">The cache key.</param>
        /// <param name="value">The value. Fullfilled if key contains in cache</param>
        /// <returns>
        /// <c>true</c> if the specified cache key contains key; otherwise, <c>false</c>.
        /// </returns>
        bool TryGet<T>(string cacheKey, out T value);



        /// <inheritdoc cref="GetOrAddFromCache{T}(Func{T}, string)"/>
        Task<T> GetOrAddFromCacheAsync<T>(Func<Task<T>> execFuncAsync, string cacheKey);

        /// <inheritdoc cref="GetOrAddFromCache{T}(Func{T}, string)"/>
        Task<T> GetOrAddFromCacheAsync<T>(Func<T> execFunc, string cacheKey);

        /// <inheritdoc cref="GetOrAddFromCache{T}(Func{T}, string, TimeSpan)"/>
        Task<T> GetOrAddFromCacheAsync<T>(Func<Task<T>> execFuncAsync, string cacheKey, TimeSpan timeout);

        /// <inheritdoc cref="GetOrAddFromCache{T}(Func{T}, string, TimeSpan)"/>
        Task<T> GetOrAddFromCacheAsync<T>(Func<T> execFunc, string cacheKey, TimeSpan timeout);

        /// <inheritdoc cref="GetCachedValue{T}(string)"/>
        Task<T> GetCachedValueAsync<T>(string cacheKey);        

        /// <inheritdoc cref="ContainsKey(string)"/>
        Task<bool> ContainsKeyAsync(string cacheKey);
               
    }
}