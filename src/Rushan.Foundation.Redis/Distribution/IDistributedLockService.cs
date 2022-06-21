using System;

namespace Rushan.Foundation.Redis.Distribution
{
    /// <summary>
    /// Service for controls starting processes in different instances.
    /// </summary>
    public interface IDistributedLockService
    {
        /// <summary>
        /// An attempt to capture flag for blocking same process in another instances
        /// </summary>
        /// <param name="lockToken">Process token</param>
        /// <param name="leaseTimeout">The flag capture expiration time</param>
        /// <param name="distributedLock">The flag</param>
        /// <returns>If flag captured return true, otherwise - false</returns>
        bool TryAcquireDistributedLock(string lockToken, TimeSpan leaseTimeout, out IDisposable distributedLock);
    }
}
