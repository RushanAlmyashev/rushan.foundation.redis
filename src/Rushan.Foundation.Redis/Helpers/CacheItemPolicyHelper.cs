using System;
using System.Runtime.Caching;

namespace Rushan.Foundation.Redis.Helpers
{
    internal static class CacheItemPolicyHelper
    {
        //Coefficient for compute of the "runtime memory cache" (level 1 cache).        
        private const double MEMORY_CACHE_TIME_COEFFICIENT = 0.322;

        /// <summary>
        /// Compute level 1 cache expiration time. 
        /// The calculated cache time depends from level 2 cache expiration time and "memory cache time coefficient"
        /// </summary>
        /// <param name="baseCacheTime">level 2 cache expiration time</param>
        /// <returns>CacheItemPolicy with new cache expiration time</returns>
        public static CacheItemPolicy ComputeCacheItemPolicy(TimeSpan? baseCacheTime)
        {
            if (baseCacheTime == null)
            {
                return GetCacheItemPolicy(TimeSpan.FromHours(1));
            }

            if (baseCacheTime > TimeSpan.FromHours(24))
            {
                baseCacheTime = TimeSpan.FromHours(24);
            }

            var cacheTime = (long)(baseCacheTime.Value.Ticks * MEMORY_CACHE_TIME_COEFFICIENT);

            return GetCacheItemPolicy(new TimeSpan(cacheTime));
        }

        /// <summary>
        /// Get CacheItemPolicy with especially behavior of cache holding
        /// </summary>
        /// <param name="cacheTime">Cache expiration time</param>
        /// <returns>CacheItemPolicy</returns>
        public static CacheItemPolicy GetCacheItemPolicy(TimeSpan cacheTime)
        {
            return new CacheItemPolicy { AbsoluteExpiration = DateTime.UtcNow.Add(cacheTime) };
        }
    }
}
