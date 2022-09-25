using Rushan.Foundation.Redis.Configuration;
using Rushan.Foundation.Redis.Logger;
using Rushan.Foundation.Redis.Persistences;
using System;
using System.Diagnostics;

namespace Rushan.Foundation.Redis.Distribution
{
    public partial class DistributedLockService : IDistributedLockService
    {
        private static readonly int ProcessID;
        private static readonly string MachineName;

        static DistributedLockService()
        {
            using (var process = Process.GetCurrentProcess())
                ProcessID = process.Id;

            MachineName = Environment.MachineName.ToLowerInvariant();
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
            var lockValue = $"{MachineName}/PID:{ProcessID}/{now:O}/{now.Add(leaseTimeout):O}";

            if (!_redisPersistence.LockTake(lockToken, lockValue, leaseTimeout))
            {
                _logger.Trace($"could not raise a distributed lock '{lockToken}' with value '{lockValue}'");
                distributedLock = null;

                return false;
            }

            distributedLock = new DistributedLock(this, lockToken, lockValue, leaseTimeout);

            _logger.Info($"successfully acquired a distributed lock '{lockToken}' with value '{lockValue}'");

            return true;
        }


        public bool TryRaiseDistributedFlag(string flagToken, TimeSpan leaseTimeout)
        {
            flagToken = $"flag:{flagToken}";

            var now = DateTime.UtcNow;
            var flagValue = $"{MachineName}/PID:{ProcessID}/{now:O}/{now.Add(leaseTimeout):O}";

            if (!_redisPersistence.LockTake(flagToken, flagValue, leaseTimeout))
            {
                _logger.Trace($"could not raise a distributed flag '{flagToken}' with value '{flagValue}'");

                return false;
            }

            _logger.Info($"successfully raised a distributed flag '{flagToken}' with value '{flagValue}'");

            return true;
        }

        internal void ReleaseDistributedLock(DistributedLock distributedLock)
        {
            var lockToken = distributedLock.LockToken;
            var lockValue = distributedLock.LockValue;

            _redisPersistence.LockRelease(lockToken, lockValue);

            _logger.Trace($"successfully released a distributed lock '{lockToken}' with lease timeout {distributedLock.LeaseTimeout}");
        }
    }
}
