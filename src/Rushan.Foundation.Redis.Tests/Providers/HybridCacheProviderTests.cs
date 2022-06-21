using Rushan.Foundation.Redis.Persistences;
using Rushan.Foundation.Redis.Providers;
using Rushan.Foundation.Redis.Serialization;
using Rushan.Foundation.Redis.Tests.Dummy;
using AutoFixture;
using Moq;
using NUnit.Framework;
using System;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace Rushan.Foundation.Redis.Tests.Providers
{
    [TestFixture]
    public class HybridCacheProviderTests
    {
        private ICacheProvider _target;

        private readonly Fixture _fixture;

        private static readonly MemoryCache _memoryCache = MemoryCache.Default;
        private DateTimeOffset _dateTimeOffset;

        private ISerializer _serializer;
        private Mock<IDummyInterface> _dummy;
        private Mock<IRedisPersistence> _redisCachePersistence;

        public HybridCacheProviderTests()
        {
            _fixture = new Fixture();            
        }

        [SetUp]
        public void SetUp()
        {
            _dummy = new Mock<IDummyInterface>();
            _serializer = new JsonSerializer();
            _redisCachePersistence = new Mock<IRedisPersistence>();
            _dateTimeOffset = DateTimeOffset.UtcNow.AddMinutes(1);

            _target = new HybridCacheProvider(_redisCachePersistence.Object, serializer: _serializer);
        }

        #region GetOrAddFromCache
        //sync
        [Test]
        public void WhenCall_GetOrAddFromCache_MemoryCacheNotEmpty_RedisCacheNotEmpty()
        {
            var key = Guid.NewGuid().ToString();
            DateTime? value = DateTime.Now;

            _redisCachePersistence.Setup(c => c.ContainsKey(It.IsAny<string>())).Returns(true);
            _redisCachePersistence.Setup(c => c.GetCachedValue(It.IsAny<string>())).Returns(_serializer.Serialize(value));
            _redisCachePersistence.Setup(c => c.KeyTimeToLive(It.IsAny<string>())).Returns(TimeSpan.FromSeconds(10));
            
            _memoryCache.Set(key, value, _dateTimeOffset);           

            var result = _target.GetOrAddFromCache(() => _dummy.Object.DummyMetod(), key);

            _redisCachePersistence.Verify(mock => mock.ContainsKey(It.IsAny<string>()), Times.Never());
            _redisCachePersistence.Verify(mock => mock.GetCachedValue(It.IsAny<string>()), Times.Never());
            _redisCachePersistence.Verify(mock => mock.KeyTimeToLive(It.IsAny<string>()), Times.Never());
            _dummy.Verify(mock => mock.DummyMetod(), Times.Never());
            
            Assert.AreEqual(result, value);
        }

        [Test]
        public void WhenCall_GetOrAddFromCache_MemoryCacheEmpty_RedisCacheNotEmpty()
        {
            var key = Guid.NewGuid().ToString();
            var value = DateTime.UtcNow;

            _redisCachePersistence.Setup(c => c.ContainsKey(It.IsAny<string>())).Returns(true);
            _redisCachePersistence.Setup(c => c.GetCachedValue(It.IsAny<string>())).Returns(_serializer.Serialize(value));
            _redisCachePersistence.Setup(c => c.KeyTimeToLive(It.IsAny<string>())).Returns(TimeSpan.FromSeconds(10));
            _dummy.Setup(c => c.DummyMetod()).Returns(value);

            var result = _target.GetOrAddFromCache(() => _dummy.Object.DummyMetod(), key);
            var memoryResult = _memoryCache.Get(key);

            _redisCachePersistence.Verify(mock => mock.ContainsKey(It.IsAny<string>()), Times.Once());
            _redisCachePersistence.Verify(mock => mock.GetCachedValue(It.IsAny<string>()), Times.Once());
            _redisCachePersistence.Verify(mock => mock.KeyTimeToLive(It.IsAny<string>()), Times.Once());
            _dummy.Verify(mock => mock.DummyMetod(), Times.Never());

            Assert.AreEqual(result, value);
            Assert.AreEqual(memoryResult, value);
        }

        [Test]
        public void WhenCall_GetOrAddFromCache_MemoryCacheEmpty_RedisCacheEmpty()
        {
            var key = Guid.NewGuid().ToString();
            var value = DateTime.UtcNow;

            _redisCachePersistence.Setup(c => c.ContainsKey(It.IsAny<string>())).Returns(false);
            _redisCachePersistence.Setup(c => c.GetCachedValue(It.IsAny<string>())).Returns(_serializer.Serialize(value));
            _redisCachePersistence.Setup(c => c.KeyTimeToLive(It.IsAny<string>())).Returns(TimeSpan.FromSeconds(10));
            _dummy.Setup(c => c.DummyMetod()).Returns(value);

            var result = _target.GetOrAddFromCache(() => _dummy.Object.DummyMetod(), key);
            var memoryResult = _memoryCache.Get(key);

            _redisCachePersistence.Verify(mock => mock.ContainsKey(It.IsAny<string>()), Times.Once());
            _redisCachePersistence.Verify(mock => mock.GetCachedValue(It.IsAny<string>()), Times.Never());
            _redisCachePersistence.Verify(mock => mock.KeyTimeToLive(It.IsAny<string>()), Times.Never());
            _dummy.Verify(mock => mock.DummyMetod(), Times.Once());

            Assert.AreEqual(result, value);
            Assert.AreEqual(memoryResult, value);
        }

        //async, sync func
        [Test]
        public async Task WhenCall_GetOrAddFromCacheAsync_MemoryCacheNotEmpty_RedisCacheNotEmpty()
        {
            var key = Guid.NewGuid().ToString();
            var value = DateTime.UtcNow;

            _redisCachePersistence.Setup(c => c.ContainsKeyAsync(It.IsAny<string>())).ReturnsAsync(true);
            _redisCachePersistence.Setup(c => c.GetCachedValueAsync(It.IsAny<string>())).ReturnsAsync(_serializer.Serialize(value));
            _redisCachePersistence.Setup(c => c.KeyTimeToLiveAsync(It.IsAny<string>())).ReturnsAsync(TimeSpan.FromSeconds(10));
            _dummy.Setup(c => c.DummyMetod()).Returns(value);
            _memoryCache.Set(key, value, _dateTimeOffset);

            var result = await _target.GetOrAddFromCacheAsync(() => _dummy.Object.DummyMetod(), key);

            _redisCachePersistence.Verify(mock => mock.ContainsKeyAsync(It.IsAny<string>()), Times.Never());
            _redisCachePersistence.Verify(mock => mock.GetCachedValueAsync(It.IsAny<string>()), Times.Never());
            _redisCachePersistence.Verify(mock => mock.KeyTimeToLiveAsync(It.IsAny<string>()), Times.Never());
            _dummy.Verify(mock => mock.DummyMetod(), Times.Never());

            Assert.AreEqual(result, value);
        }

        [Test]
        public async Task WhenCall_GetOrAddFromCacheAsync_MemoryCacheEmpty_RedisCacheNotEmpty()
        {
            var key = Guid.NewGuid().ToString();
            var value = DateTime.UtcNow;

            _redisCachePersistence.Setup(c => c.ContainsKeyAsync(It.IsAny<string>())).ReturnsAsync(true);
            _redisCachePersistence.Setup(c => c.GetCachedValueAsync(It.IsAny<string>())).ReturnsAsync(_serializer.Serialize(value));
            _redisCachePersistence.Setup(c => c.KeyTimeToLiveAsync(It.IsAny<string>())).ReturnsAsync(TimeSpan.FromSeconds(10));
            _dummy.Setup(c => c.DummyMetod()).Returns(value);

            var result = await _target.GetOrAddFromCacheAsync(() => _dummy.Object.DummyMetod(), key);
            var memoryResult = _memoryCache.Get(key);

            _redisCachePersistence.Verify(mock => mock.ContainsKeyAsync(It.IsAny<string>()), Times.Once());
            _redisCachePersistence.Verify(mock => mock.GetCachedValueAsync(It.IsAny<string>()), Times.Once());
            _redisCachePersistence.Verify(mock => mock.KeyTimeToLiveAsync(It.IsAny<string>()), Times.Once());
            _dummy.Verify(mock => mock.DummyMetod(), Times.Never());

            Assert.AreEqual(result, value);
            Assert.AreEqual(memoryResult, value);
        }

        [Test]
        public async Task WhenCall_GetOrAddFromCacheAsync_MemoryCacheEmpty_RedisCacheEmpty()
        {
            var key = Guid.NewGuid().ToString();
            var value = DateTime.UtcNow;

            _redisCachePersistence.Setup(c => c.ContainsKeyAsync(It.IsAny<string>())).ReturnsAsync(false);
            _redisCachePersistence.Setup(c => c.GetCachedValueAsync(It.IsAny<string>())).ReturnsAsync(_serializer.Serialize(value));
            _redisCachePersistence.Setup(c => c.KeyTimeToLiveAsync(It.IsAny<string>())).ReturnsAsync(TimeSpan.FromSeconds(10));
            _dummy.Setup(c => c.DummyMetod()).Returns(value);

            var result = await _target.GetOrAddFromCacheAsync(() => _dummy.Object.DummyMetod(), key);
            var memoryResult = _memoryCache.Get(key);

            _redisCachePersistence.Verify(mock => mock.ContainsKeyAsync(It.IsAny<string>()), Times.Once());
            _redisCachePersistence.Verify(mock => mock.GetCachedValueAsync(It.IsAny<string>()), Times.Never());
            _redisCachePersistence.Verify(mock => mock.KeyTimeToLiveAsync(It.IsAny<string>()), Times.Never());
            _dummy.Verify(mock => mock.DummyMetod(), Times.Once());

            Assert.AreEqual(result, value);
            Assert.AreEqual(memoryResult, value);
        }

        //async, async func
        [Test]
        public async Task WhenCall_GetOrAddFromCacheAsync_WithAsyncFunc_MemoryCacheNotEmpty_RedisCacheNotEmpty()
        {
            var key = Guid.NewGuid().ToString();
            var value = DateTime.UtcNow;

            _redisCachePersistence.Setup(c => c.ContainsKeyAsync(It.IsAny<string>())).ReturnsAsync(true);
            _redisCachePersistence.Setup(c => c.GetCachedValueAsync(It.IsAny<string>())).ReturnsAsync(_serializer.Serialize(value));
            _redisCachePersistence.Setup(c => c.KeyTimeToLiveAsync(It.IsAny<string>())).ReturnsAsync(TimeSpan.FromSeconds(10));
            _dummy.Setup(c => c.DummyMetodAsync()).ReturnsAsync(value);
            _memoryCache.Set(key, value, _dateTimeOffset);

            var result = await _target.GetOrAddFromCacheAsync(() => _dummy.Object.DummyMetodAsync(), key);

            _redisCachePersistence.Verify(mock => mock.ContainsKeyAsync(It.IsAny<string>()), Times.Never());
            _redisCachePersistence.Verify(mock => mock.GetCachedValueAsync(It.IsAny<string>()), Times.Never());
            _redisCachePersistence.Verify(mock => mock.KeyTimeToLiveAsync(It.IsAny<string>()), Times.Never());
            _dummy.Verify(mock => mock.DummyMetodAsync(), Times.Never());

            Assert.AreEqual(result, value);
        }

        [Test]
        public async Task WhenCall_GetOrAddFromCacheAsync_WithAsyncFunc_MemoryCacheEmpty_RedisCacheNotEmpty()
        {
            var key = Guid.NewGuid().ToString();
            var value = DateTime.UtcNow;

            _redisCachePersistence.Setup(c => c.ContainsKeyAsync(It.IsAny<string>())).ReturnsAsync(true);
            _redisCachePersistence.Setup(c => c.GetCachedValueAsync(It.IsAny<string>())).ReturnsAsync(_serializer.Serialize(value));
            _redisCachePersistence.Setup(c => c.KeyTimeToLiveAsync(It.IsAny<string>())).ReturnsAsync(TimeSpan.FromSeconds(10));
            _dummy.Setup(c => c.DummyMetodAsync()).ReturnsAsync(value);

            var result = await _target.GetOrAddFromCacheAsync(() => _dummy.Object.DummyMetodAsync(), key);
            var memoryResult = _memoryCache.Get(key);

            _redisCachePersistence.Verify(mock => mock.ContainsKeyAsync(It.IsAny<string>()), Times.Once());
            _redisCachePersistence.Verify(mock => mock.GetCachedValueAsync(It.IsAny<string>()), Times.Once());
            _redisCachePersistence.Verify(mock => mock.KeyTimeToLiveAsync(It.IsAny<string>()), Times.Once());
            _dummy.Verify(mock => mock.DummyMetodAsync(), Times.Never());

            Assert.AreEqual(result, value);
            Assert.AreEqual(memoryResult, value);
        }

        [Test]
        public async Task WhenCall_GetOrAddFromCacheAsync_WithAsyncFunc_MemoryCacheEmpty_RedisCacheEmpty()
        {
            var key = Guid.NewGuid().ToString();
            var value = DateTime.UtcNow;

            _redisCachePersistence.Setup(c => c.ContainsKeyAsync(It.IsAny<string>())).ReturnsAsync(false);
            _redisCachePersistence.Setup(c => c.GetCachedValueAsync(It.IsAny<string>())).ReturnsAsync(_serializer.Serialize(value));
            _redisCachePersistence.Setup(c => c.KeyTimeToLiveAsync(It.IsAny<string>())).ReturnsAsync(TimeSpan.FromSeconds(10));
            _dummy.Setup(c => c.DummyMetodAsync()).ReturnsAsync(value);

            var result = await _target.GetOrAddFromCacheAsync(() => _dummy.Object.DummyMetodAsync(), key);
            var memoryResult = _memoryCache.Get(key);

            _redisCachePersistence.Verify(mock => mock.ContainsKeyAsync(It.IsAny<string>()), Times.Once());
            _redisCachePersistence.Verify(mock => mock.GetCachedValueAsync(It.IsAny<string>()), Times.Never());
            _redisCachePersistence.Verify(mock => mock.KeyTimeToLiveAsync(It.IsAny<string>()), Times.Never());
            _dummy.Verify(mock => mock.DummyMetodAsync(), Times.Once());

            Assert.AreEqual(result, value);
            Assert.AreEqual(memoryResult, value);
        }
        #endregion

        #region GetCachedValue

        [Test]
        public void WhenCall_GetCachedValue_MemoryCacheNotEmpty_RedisCacheNotEmpty()
        {
            var key = Guid.NewGuid().ToString();
            var value = DateTime.UtcNow;

            _redisCachePersistence.Setup(c => c.GetCachedValue(It.IsAny<string>())).Returns(_serializer.Serialize(value));
            _redisCachePersistence.Setup(c => c.KeyTimeToLive(It.IsAny<string>())).Returns(TimeSpan.FromSeconds(10));
            _memoryCache.Set(key, value, _dateTimeOffset);

            var result = _target.GetCachedValue<DateTime>(key);

            _redisCachePersistence.Verify(mock => mock.GetCachedValue(It.IsAny<string>()), Times.Never());
            _redisCachePersistence.Verify(mock => mock.KeyTimeToLive(It.IsAny<string>()), Times.Never());
            Assert.AreEqual(result, value);
        }

        [Test]
        public void WhenCall_GetCachedValue_MemoryCacheEmpty_RedisCacheNotEmpty()
        {
            var key = Guid.NewGuid().ToString();
            var value = DateTime.UtcNow;

            _redisCachePersistence.Setup(c => c.GetCachedValue(It.IsAny<string>())).Returns(_serializer.Serialize(value));
            _redisCachePersistence.Setup(c => c.KeyTimeToLive(It.IsAny<string>())).Returns(TimeSpan.FromSeconds(10));

            var result = _target.GetCachedValue<DateTime>(key);
            var memoryResult = _memoryCache.Get(key);

            _redisCachePersistence.Verify(mock => mock.GetCachedValue(It.IsAny<string>()), Times.Once());
            _redisCachePersistence.Verify(mock => mock.KeyTimeToLive(It.IsAny<string>()), Times.Once());
            Assert.AreEqual(result, value);
            Assert.AreEqual(memoryResult, value);
        }

        [Test]
        public void WhenCall_GetCachedValue_MemoryCacheEmpty_RedisCacheEmpty()
        {
            var key = Guid.NewGuid().ToString();
            var value = DateTime.UtcNow;

            _redisCachePersistence.Setup(c => c.GetCachedValue(It.IsAny<string>())).Throws(new Exception());
            
            Assert.Throws<Exception>(() => _target.GetCachedValue<DateTime>(key));

            _redisCachePersistence.Verify(mock => mock.GetCachedValue(It.IsAny<string>()), Times.Once());
            _redisCachePersistence.Verify(mock => mock.KeyTimeToLive(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public async Task WhenCall_GetCachedValueAsync_MemoryCacheNotEmpty_RedisCacheNotEmpty()
        {
            var key = Guid.NewGuid().ToString();
            var value = DateTime.UtcNow;

            _redisCachePersistence.Setup(c => c.GetCachedValueAsync(It.IsAny<string>())).ReturnsAsync(_serializer.Serialize(value));
            _redisCachePersistence.Setup(c => c.KeyTimeToLiveAsync(It.IsAny<string>())).ReturnsAsync(TimeSpan.FromSeconds(10));
            _memoryCache.Set(key, value, _dateTimeOffset);

            var result = await _target.GetCachedValueAsync<DateTime>(key);

            _redisCachePersistence.Verify(mock => mock.GetCachedValueAsync(It.IsAny<string>()), Times.Never());
            _redisCachePersistence.Verify(mock => mock.KeyTimeToLiveAsync(It.IsAny<string>()), Times.Never());
            Assert.AreEqual(result, value);
        }

        [Test]
        public async Task WhenCall_GetCachedValueAsync_MemoryCacheEmpty_RedisCacheNotEmpty()
        {
            var key = Guid.NewGuid().ToString();
            var value = DateTime.UtcNow;

            _redisCachePersistence.Setup(c => c.GetCachedValueAsync(It.IsAny<string>())).ReturnsAsync(_serializer.Serialize(value));
            _redisCachePersistence.Setup(c => c.KeyTimeToLiveAsync(It.IsAny<string>())).ReturnsAsync(TimeSpan.FromSeconds(10));

            var result = await _target.GetCachedValueAsync<DateTime>(key);
            var memoryResult = (DateTime)_memoryCache.Get(key);

            _redisCachePersistence.Verify(mock => mock.GetCachedValueAsync(It.IsAny<string>()), Times.Once());
            _redisCachePersistence.Verify(mock => mock.KeyTimeToLiveAsync(It.IsAny<string>()), Times.Once());
            Assert.AreEqual(result, value);
            Assert.AreEqual(memoryResult, value);
        }

        [Test]
        public void WhenCall_GetCachedValueAsync_MemoryCacheEmpty_RedisCacheEmpty()
        {
            var key = Guid.NewGuid().ToString();
            var value = DateTime.UtcNow;

            _redisCachePersistence.Setup(c => c.GetCachedValueAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

            Assert.ThrowsAsync<Exception>(async () => await _target.GetCachedValueAsync<DateTime>(key));
            _redisCachePersistence.Verify(mock => mock.GetCachedValueAsync(It.IsAny<string>()), Times.Once());
            _redisCachePersistence.Verify(mock => mock.KeyTimeToLiveAsync(It.IsAny<string>()), Times.Never());
        }

        #endregion

        #region TryGet
        [Test]
        public void WhenCall_TryGet_MemoryCacheNotEmpty_RedisCacheNotEmpty()
        {
            var key = Guid.NewGuid().ToString();
            var value = DateTime.UtcNow;
            var serializedValue = _serializer.Serialize(value);

            _memoryCache.Set(key, value, _dateTimeOffset);
            _redisCachePersistence.Setup(c => c.ContainsKey(It.IsAny<string>())).Returns(true);
            _redisCachePersistence.Setup(c => c.GetCachedValue(It.IsAny<string>())).Returns(serializedValue);
            _redisCachePersistence.Setup(c => c.KeyTimeToLive(It.IsAny<string>())).Returns(TimeSpan.FromSeconds(10));

            DateTime actualCacheValue = default;
            var actual = _target.TryGet(key, out actualCacheValue);
            var memoryResult = (DateTime)_memoryCache.Get(key);

            _redisCachePersistence.Verify(mock => mock.ContainsKey(It.IsAny<string>()), Times.Never());
            _redisCachePersistence.Verify(mock => mock.GetCachedValue(It.IsAny<string>()), Times.Never());
            _redisCachePersistence.Verify(mock => mock.KeyTimeToLive(It.IsAny<string>()), Times.Never());
            Assert.AreEqual(true, actual);
            Assert.AreEqual(value, actualCacheValue);
            Assert.AreEqual(value, memoryResult);
        }

        [Test]
        public void WhenCall_TryGet_MemoryCacheEmpty_RedisCacheNotEmpty()
        {
            var key = Guid.NewGuid().ToString();
            var value = DateTime.UtcNow;
            var serializedValue = _serializer.Serialize(value);

            _redisCachePersistence.Setup(c => c.ContainsKey(It.IsAny<string>())).Returns(true);
            _redisCachePersistence.Setup(c => c.GetCachedValue(It.IsAny<string>())).Returns(serializedValue);
            _redisCachePersistence.Setup(c => c.KeyTimeToLive(It.IsAny<string>())).Returns(TimeSpan.FromSeconds(10));

            DateTime actualCacheValue = default;
            var actual = _target.TryGet(key, out actualCacheValue);
            var memoryResult = (DateTime)_memoryCache.Get(key);

            _redisCachePersistence.Verify(mock => mock.ContainsKey(It.IsAny<string>()), Times.Once());
            _redisCachePersistence.Verify(mock => mock.GetCachedValue(It.IsAny<string>()), Times.Once());
            _redisCachePersistence.Verify(mock => mock.KeyTimeToLive(It.IsAny<string>()), Times.Once());
            Assert.AreEqual(true, actual);
            Assert.AreEqual(value, actualCacheValue);
            Assert.AreEqual(value, memoryResult);
        }

        [Test]
        public void WhenCall_TryGet_MemoryCacheEmpty_RedisCacheEmpty()
        {
            var key = Guid.NewGuid().ToString();
            var value = DateTime.UtcNow;
            var serializedValue = _serializer.Serialize(value);

            
            _redisCachePersistence.Setup(c => c.ContainsKey(It.IsAny<string>())).Returns(false);
            _redisCachePersistence.Setup(c => c.GetCachedValue(It.IsAny<string>())).Returns((byte[])null);
            _redisCachePersistence.Setup(c => c.KeyTimeToLive(It.IsAny<string>())).Returns((TimeSpan?)null);

            DateTime actualCacheValue = default;
            var actual = _target.TryGet(key, out actualCacheValue);

            _redisCachePersistence.Verify(mock => mock.ContainsKey(It.IsAny<string>()), Times.Once);
            _redisCachePersistence.Verify(mock => mock.GetCachedValue(It.IsAny<string>()), Times.Never());
            _redisCachePersistence.Verify(mock => mock.KeyTimeToLive(It.IsAny<string>()), Times.Never());
            Assert.AreEqual(false, actual);
            Assert.AreEqual((DateTime)default, actualCacheValue);
        }

        #endregion

        #region Contains

        [Test]
        public void WhenCall_Contains_MemoryCacheNotEmpty_RedisCacheNotEmpty()
        {
            var key = Guid.NewGuid().ToString();
            var value = DateTime.UtcNow;

            _memoryCache.Set(key, value, _dateTimeOffset);
            _redisCachePersistence.Setup(c => c.ContainsKey(key)).Returns(true);

            var actual = _target.ContainsKey(key);

            _redisCachePersistence.Verify(mock => mock.ContainsKey(It.IsAny<string>()), Times.Never());            
            Assert.AreEqual(true, actual);
        }

        [Test]
        public void WhenCall_Contains_MemoryCacheEmpty_RedisCacheNotEmpty()
        {
            var key = Guid.NewGuid().ToString();
            var value = DateTime.UtcNow;

            _redisCachePersistence.Setup(c => c.ContainsKey(key)).Returns(true);

            var actual = _target.ContainsKey(key);

            _redisCachePersistence.Verify(mock => mock.ContainsKey(It.IsAny<string>()), Times.Once());
            Assert.AreEqual(true, actual);
        }

        [Test]
        public void WhenCall_Contains_MemoryCacheEmpty_RedisCacheEmpty()
        {
            var key = Guid.NewGuid().ToString();
            var value = DateTime.UtcNow;

            _redisCachePersistence.Setup(c => c.ContainsKey(key)).Returns(false);

            var actual = _target.ContainsKey(key);

            _redisCachePersistence.Verify(mock => mock.ContainsKey(It.IsAny<string>()), Times.Once());
            Assert.AreEqual(false, actual);
        }

        [Test]
        public async Task WhenCall_ContainsAsync_MemoryCacheNotEmpty_RedisCacheNotEmpty()
        {
            var key = Guid.NewGuid().ToString();
            var value = DateTime.UtcNow;

            _memoryCache.Set(key, value, _dateTimeOffset);
            _redisCachePersistence.Setup(c => c.ContainsKeyAsync(key)).ReturnsAsync(true);

            var actual = await _target.ContainsKeyAsync(key);

            _redisCachePersistence.Verify(mock => mock.ContainsKeyAsync(It.IsAny<string>()), Times.Never());
            Assert.AreEqual(true, actual);
        }

        [Test]
        public async Task WhenCall_ContainsAsync_MemoryCacheEmpty_RedisCacheNotEmpty()
        {
            var key = Guid.NewGuid().ToString();
            var value = DateTime.UtcNow;

            _redisCachePersistence.Setup(c => c.ContainsKeyAsync(key)).ReturnsAsync(true);

            var actual = await _target.ContainsKeyAsync(key);

            _redisCachePersistence.Verify(mock => mock.ContainsKeyAsync(It.IsAny<string>()), Times.Once());
            Assert.AreEqual(true, actual);
        }

        [Test]
        public async Task WhenCall_ContainsAsync_MemoryCacheEmpty_RedisCacheEmpty()
        {
            var key = Guid.NewGuid().ToString();
            var value = DateTime.UtcNow;

            _redisCachePersistence.Setup(c => c.ContainsKeyAsync(key)).ReturnsAsync(false);

            var actual = await _target.ContainsKeyAsync(key);

            _redisCachePersistence.Verify(mock => mock.ContainsKeyAsync(It.IsAny<string>()), Times.Once());
            Assert.AreEqual(false, actual);
        }

        #endregion
    }
}
