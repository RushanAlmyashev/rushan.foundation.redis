using Rushan.Foundation.Redis.Configuration;
using Rushan.Foundation.Redis.Logger;
using Rushan.Foundation.Redis.Persistences;
using System;
using System.Diagnostics;

namespace Rushan.Foundation.Redis.Distribution
{
    public class DistributedLockService : IDistributedLockService
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

        private readonly IRedisPersistence _redisPersistence;
        private readonly ILogger _logger;

        public DistributedLockService(RedisConfigurationSection redisConfigurationSection, ILogger logger = null)
        {
            _logger = logger ?? new EmptyLogger();

            _redisPersistence = new RedisPersistence(redisConfigurationSection.ConnectionString, _logger);
        }

        public DistributedLockService(string connectionString, ILogger logger = null)
        {
            _logger = logger ?? new EmptyLogger();

            _redisPersistence = new RedisPersistence(connectionString, _logger);
        }

        public bool TryAcquireDistributedLock(string lockToken, TimeSpan leaseTimeout, out IDisposable distributedLock)
        {
            lockToken = $"lock:{lockToken}";

            var now = DateTime.UtcNow;
            var lockValue = $"{Environment.MachineName.ToUpperInvariant()}/PID:{Process.GetCurrentProcess().Id}/{now:O}/{now.Add(leaseTimeout):O}";

            if (!_redisPersistence.LockTake(lockToken, lockValue, leaseTimeout))
            {
                distributedLock = null;
                
                return false;
            } 

            distributedLock = new DistributedLock(this, lockToken, lockValue, leaseTimeout);
            return true;
        }

        private void ReleaseDistributedLock(DistributedLock distributedLock)
        {
            var lockToken = distributedLock.LockToken;
            var lockValue = distributedLock.LockValue;
            
            _redisPersistence.LockRelease(lockToken, lockValue);
        }
    }
}
