using NUnit.Framework;
using Foundation.Redis.Distribution;
using System;
using System.Threading;
using Foundation.Redis.Persistences;
using Foundation.Redis.Logger;
using Foundation.Redis.Providers;

namespace Foundation.Redis.Tests.Distribution
{
    [TestFixture]
    [Category("Integration")]
    [Ignore("Integration")]
    public class DistributedLockServiceTests
    {
        [Test]        
        public void TryAcquireDistributedLock()
        {
            var connectionString = "Yours db redis connection string";
            
            var distributedLockService = new DistributedLockService(connectionString);

            var lockToken = Guid.NewGuid().ToString("n");

            IDisposable distributedLock1, distributedLock2;
            Assert.IsTrue(distributedLockService.TryAcquireDistributedLock(lockToken, TimeSpan.FromMinutes(1), out distributedLock1));
            Assert.IsFalse(distributedLockService.TryAcquireDistributedLock(lockToken, TimeSpan.FromMinutes(1), out distributedLock2));

            using (distributedLock1) { }
            using (distributedLock2) { }

            IDisposable distributedLock3;
            Assert.IsTrue(distributedLockService.TryAcquireDistributedLock(Guid.NewGuid().ToString("n"), TimeSpan.FromSeconds(1), out distributedLock3));

            using (distributedLock3)
            {
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }
        }
    }
}
