using Rushan.Foundation.Redis.Providers.Impl;
using AutoFixture;
using Moq;
using NUnit.Framework;
using Rushan.Foundation.Redis.Tests.Dummy;
using System;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Rushan.Foundation.Redis.Providers;

namespace Rushan.Foundation.Redis.Tests.Providers
{
    [TestFixture]
    public class MemoryCacheProviderTests
    {
        private ICacheProvider _target;

        private readonly Fixture _fixture;

        private static readonly MemoryCache _memoryCache = MemoryCache.Default;
        private DateTimeOffset _dateTimeOffset;

        private Mock<IDummyInterface> _dummy;

        public MemoryCacheProviderTests()
        {
            _fixture = new Fixture();
        }

        [SetUp]
        public void SetUp()
        {
            _dummy = new Mock<IDummyInterface>();
            _dateTimeOffset = DateTimeOffset.UtcNow.AddMinutes(1);

            _target = new MemoryCacheProvider(_fixture.Create<TimeSpan>());
        }

        #region GetOrAddFromCache
        //sync
        [Test]
        public void WhenCall_GetOrAddFromCache_MemoryCacheNotEmpty()
        {
            var key = Guid.NewGuid().ToString();
            DateTime? value = DateTime.Now;

            _memoryCache.Set(key, value, _dateTimeOffset);

            var result = _target.GetOrAddFromCache(() => _dummy.Object.DummyMetod(), key);

            _dummy.Verify(mock => mock.DummyMetod(), Times.Never());

            Assert.AreEqual(result, value);
        }


        [Test]
        public void WhenCall_GetOrAddFromCache_MemoryCacheEmpty()
        {
            var key = Guid.NewGuid().ToString();
            var value = DateTime.UtcNow;

            _dummy.Setup(c => c.DummyMetod()).Returns(value);

            var result = _target.GetOrAddFromCache(() => _dummy.Object.DummyMetod(), key);

            _dummy.Verify(mock => mock.DummyMetod(), Times.Once());

            Assert.AreEqual(result, value);
        }

        //async, sync func
        [Test]
        public async Task WhenCall_GetOrAddFromCacheAsync_MemoryCacheNotEmpty()
        {
            var key = Guid.NewGuid().ToString();
            var value = DateTime.UtcNow;

            _dummy.Setup(c => c.DummyMetod()).Returns(value);
            _memoryCache.Set(key, value, _dateTimeOffset);

            var result = await _target.GetOrAddFromCacheAsync(() => _dummy.Object.DummyMetod(), key);

            _dummy.Verify(mock => mock.DummyMetod(), Times.Never());

            Assert.AreEqual(result, value);
        }


        [Test]
        public async Task WhenCall_GetOrAddFromCacheAsync_MemoryCacheEmpty()
        {
            var key = Guid.NewGuid().ToString();
            var value = DateTime.UtcNow;

            _dummy.Setup(c => c.DummyMetod()).Returns(value);

            var result = await _target.GetOrAddFromCacheAsync(() => _dummy.Object.DummyMetod(), key);

            _dummy.Verify(mock => mock.DummyMetod(), Times.Once());

            Assert.AreEqual(result, value);
        }

        //async, async func
        [Test]
        public async Task WhenCall_GetOrAddFromCacheAsync_WithAsyncFunc_MemoryCacheNotEmpty()
        {
            var key = Guid.NewGuid().ToString();
            var value = DateTime.UtcNow;

            _dummy.Setup(c => c.DummyMetodAsync()).ReturnsAsync(value);
            _memoryCache.Set(key, value, _dateTimeOffset);

            var result = await _target.GetOrAddFromCacheAsync(() => _dummy.Object.DummyMetodAsync(), key);

            _dummy.Verify(mock => mock.DummyMetodAsync(), Times.Never());

            Assert.AreEqual(result, value);
        }


        [Test]
        public async Task WhenCall_GetOrAddFromCacheAsync_WithAsyncFunc_MemoryCacheEmpty()
        {
            var key = Guid.NewGuid().ToString();
            var value = DateTime.UtcNow;

            _dummy.Setup(c => c.DummyMetodAsync()).ReturnsAsync(value);

            var result = await _target.GetOrAddFromCacheAsync(() => _dummy.Object.DummyMetodAsync(), key);

            _dummy.Verify(mock => mock.DummyMetodAsync(), Times.Once());

            Assert.AreEqual(result, value);
        }

        #endregion

        #region GetCachedValue

        [Test]
        public void WhenCall_GetCachedValue_MemoryCacheNotEmpty()
        {
            var key = Guid.NewGuid().ToString();
            var value = DateTime.UtcNow;

            _memoryCache.Set(key, value, _dateTimeOffset);

            var result = _target.GetCachedValue<DateTime>(key);

            Assert.AreEqual(result, value);
        }


        [Test]
        public void WhenCall_GetCachedValue_MemoryCacheEmpty()
        {
            var key = Guid.NewGuid().ToString();
            var value = DateTime.UtcNow;

            Assert.Throws<InvalidOperationException>(() => _target.GetCachedValue<DateTime>(key));
        }

        [Test]
        public async Task WhenCall_GetCachedValueAsync_MemoryCacheNotEmpty()
        {
            var key = Guid.NewGuid().ToString();
            var value = DateTime.UtcNow;

            _memoryCache.Set(key, value, _dateTimeOffset);

            var result = await _target.GetCachedValueAsync<DateTime>(key);

            Assert.AreEqual(result, value);
        }


        [Test]
#pragma warning disable 1998
        public async Task WhenCall_GetCachedValueAsync_MemoryCacheEmpty()
#pragma warning restore 1998
        {
            var key = Guid.NewGuid().ToString();
            var value = DateTime.UtcNow;


            Assert.ThrowsAsync<InvalidOperationException>(async () => await _target.GetCachedValueAsync<DateTime>(key));
        }

        #endregion

        #region TryGet
        [Test]
        public void WhenCall_TryGet_MemoryCacheNotEmpty()
        {
            var key = Guid.NewGuid().ToString();
            var value = DateTime.UtcNow;

            _memoryCache.Set(key, value, _dateTimeOffset);

            DateTime actualCacheValue = default;
            var actual = _target.TryGet(key, out actualCacheValue);

            Assert.AreEqual(true, actual);
            Assert.AreEqual(value, actualCacheValue);
        }

        [Test]
        public void WhenCall_TryGet_MemoryCacheEmpty()
        {
            var key = Guid.NewGuid().ToString();
            var value = DateTime.UtcNow;

            DateTime actualCacheValue = default;
            var actual = _target.TryGet(key, out actualCacheValue);

            Assert.AreEqual(false, actual);
            Assert.AreEqual((DateTime)default, actualCacheValue);
        }

        #endregion

        #region Contains

        [Test]
        public void WhenCall_Contains_MemoryCacheNotEmpty()
        {
            var key = Guid.NewGuid().ToString();
            var value = DateTime.UtcNow;

            _memoryCache.Set(key, value, _dateTimeOffset);

            var actual = _target.ContainsKey(key);

            Assert.AreEqual(true, actual);
        }


        [Test]
        public void WhenCall_Contains_MemoryCacheEmpty_RedisCacheEmpty()
        {
            var key = Guid.NewGuid().ToString();

            var actual = _target.ContainsKey(key);

            Assert.AreEqual(false, actual);
        }

        [Test]
        public async Task WhenCall_ContainsAsync_MemoryCacheNotEmpty()
        {
            var key = Guid.NewGuid().ToString();
            var value = DateTime.UtcNow;

            _memoryCache.Set(key, value, _dateTimeOffset);

            var actual = await _target.ContainsKeyAsync(key);

            Assert.AreEqual(true, actual);
        }

        [Test]
        public async Task WhenCall_ContainsAsync_MemoryCacheEmpty()
        {
            var key = Guid.NewGuid().ToString();
            var value = DateTime.UtcNow;

            var actual = await _target.ContainsKeyAsync(key);

            Assert.AreEqual(false, actual);
        }

        #endregion
    }
}
