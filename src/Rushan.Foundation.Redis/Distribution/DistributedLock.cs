using System;

namespace Rushan.Foundation.Redis.Distribution
{
    internal class DistributedLock : IDisposable
    {
        private readonly DistributedLockService _distributedLockService;

        public string LockToken { get; }
        public string LockValue { get; }
        public TimeSpan LeaseTimeout { get; }

        public DistributedLock(DistributedLockService distributedLockService, string lockToken, string lockValue, TimeSpan leaseTimeout)
        {
            _distributedLockService = distributedLockService;

            LockToken = lockToken;
            LockValue = lockValue;
            LeaseTimeout = leaseTimeout;
        }

        public void Dispose()
        {
            _distributedLockService.ReleaseDistributedLock(this);
        }
    }
}
