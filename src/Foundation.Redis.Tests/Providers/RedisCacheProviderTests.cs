using Foundation.Redis.Logger;
using Foundation.Redis.Persistences;
using Foundation.Redis.Providers;
using Foundation.Redis.Serialization;
using Foundation.Redis.Tests.Dummy;
using AutoFixture;
using Moq;
using NUnit.Framework;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Foundation.Redis.Tests.Providers
{
    [TestFixture]
    [Category("Unit")]
    public class RedisCacheProviderTests
    {
        private readonly Fixture _fixture;

        private IRedisCacheProvider _target;

        private Mock<IDummyInterface> _dummy;
        private Mock<ILogger> _logger;
        private Mock<ISerializer> _serializer;
        private Mock<IRedisPersistence> _redisPersistence;

        public RedisCacheProviderTests()
        {
            _fixture = new Fixture();
        }

        [SetUp]
        public void SetUp()
        {
            _logger = new Mock<ILogger>();
            _serializer = new Mock<ISerializer>();
            _redisPersistence = new Mock<IRedisPersistence>();
            _dummy = new Mock<IDummyInterface>();

            _target = new RedisCacheProvider(_redisPersistence.Object, _logger.Object, _serializer.Object);
        }

        [Test]
        public void WhenCallGetOrAddFromCache_WithValueInCache_DoNotCallMethod()
        {
            var date = DateTime.Now;
            var dataPayload = Guid.NewGuid().ToByteArray();
            var cacheKey = Guid.NewGuid().ToString();

            _serializer.Setup(c => c.Deserialize<DateTime>(dataPayload)).Returns(date);
            _redisPersistence.Setup(c => c.ContainsKey(cacheKey)).Returns(true);
            _redisPersistence.Setup(c => c.GetCachedValue(cacheKey)).Returns(dataPayload);           

            var actual = _target.GetOrAddFromCache(() => _dummy.Object.DummyMetod(), cacheKey);

            _dummy.Verify(mock => mock.DummyMetod(), Times.Never());
            Assert.AreEqual(actual, date);
        }

        [Test]
        public void WhenCallGetOrAddFromCache_WithNoValueInCache_CallMethod()
        {
            var date = DateTime.Now;
            var dataPayload = Guid.NewGuid().ToByteArray();
            var cacheKey = Guid.NewGuid().ToString();

            _serializer.Setup(c => c.Deserialize<DateTime>(dataPayload)).Returns(date);
            _redisPersistence.Setup(c => c.ContainsKey(cacheKey)).Returns(false);
            _dummy.Setup(c => c.DummyMetod()).Returns(date);

            var actual = _target.GetOrAddFromCache(() => _dummy.Object.DummyMetod(), cacheKey);

            _dummy.Verify(mock => mock.DummyMetod(), Times.Once());            
            Assert.AreEqual(actual, date);
        }

        [Test]
        public async Task WhenCallGetOrAddFromCacheAsync_WithValueInCache_DoNotCallMethod()
        {
            var date = DateTime.Now;
            var dataPayload = Guid.NewGuid().ToByteArray();
            var cacheKey = Guid.NewGuid().ToString();

            _serializer.Setup(c => c.Deserialize<DateTime>(dataPayload)).Returns(date);
            _redisPersistence.Setup(c => c.ContainsKeyAsync(cacheKey)).ReturnsAsync(true);
            _redisPersistence.Setup(c => c.GetCachedValueAsync(cacheKey)).ReturnsAsync(dataPayload);

            var actual = await _target.GetOrAddFromCacheAsync(() => _dummy.Object.DummyMetodAsync(), cacheKey, TimeSpan.FromSeconds(10));

            _dummy.Verify(mock => mock.DummyMetodAsync(), Times.Never());            
            Assert.AreEqual(actual, date);
        }

        [Test]
        public async Task WhenCallGetOrAddFromCacheAsync_WithNoValueInCache_CallMethod()
        {
            var date = DateTime.Now;
            var dataPayload = Guid.NewGuid().ToByteArray();
            var cacheKey = Guid.NewGuid().ToString();

            _serializer.Setup(c => c.Deserialize<DateTime>(dataPayload)).Returns(date);
            _redisPersistence.Setup(c => c.ContainsKeyAsync(cacheKey)).ReturnsAsync(false);
            _dummy.Setup(c => c.DummyMetodAsync()).ReturnsAsync(date);

            var actual = await _target.GetOrAddFromCacheAsync(() => _dummy.Object.DummyMetodAsync(), cacheKey, TimeSpan.FromSeconds(10));

            _dummy.Verify(mock => mock.DummyMetodAsync(), Times.Once());
            Assert.AreEqual(actual, date);
        }

        [Test]
        public void WhenCallAddOrUpdate_DoesNotThrow()
        {
            var date = DateTime.Now;

            var dataPayload = Guid.NewGuid().ToByteArray();
            var cacheKey = Guid.NewGuid().ToString();

            _serializer.Setup(c => c.Serialize(date)).Returns(dataPayload);            

            Assert.DoesNotThrow(() =>
            {
                _target.AddOrUpdateValue(cacheKey, date, TimeSpan.FromSeconds(10));
            });
        }

        [Test]
        public void WhenCallAddOrUpdateAsync_DoesNotThrow()
        {
            var date = DateTime.Now;

            var dataPayload = Guid.NewGuid().ToByteArray();
            var cacheKey = Guid.NewGuid().ToString();

            _serializer.Setup(c => c.Serialize(date)).Returns(dataPayload);

            Assert.DoesNotThrowAsync(async () =>
            {
                await _target.AddOrUpdateValueAsync(cacheKey, date, TimeSpan.FromSeconds(10));
            });
        }

        [Test]
        public void WhenCallGetCachedValue_ReturnExpectedValue()
        {
            var date = DateTime.Now;

            var dataPayload = Guid.NewGuid().ToByteArray();
            var cacheKey = Guid.NewGuid().ToString();

            _serializer.Setup(c => c.Deserialize<DateTime>(dataPayload)).Returns(date);
            _redisPersistence.Setup(c => c.GetCachedValue(cacheKey)).Returns(dataPayload);

            var actual = _target.GetCachedValue<DateTime>(cacheKey);

            Assert.AreEqual(actual, date);
        }

        [Test]
        public async Task WhenCallGetCachedValueAsync_ReturnExpectedValue()
        {
            var date = DateTime.Now;

            var dataPayload = Guid.NewGuid().ToByteArray();
            var cacheKey = Guid.NewGuid().ToString();

            _serializer.Setup(c => c.Deserialize<DateTime>(dataPayload)).Returns(date);
            _redisPersistence.Setup(c => c.GetCachedValueAsync(cacheKey)).ReturnsAsync(dataPayload);

            var actual = await _target.GetCachedValueAsync<DateTime>(cacheKey);

            Assert.AreEqual(actual, date);
        }

        [Test]
        public void WhenCallGetCachedValue_ValueIsEmpty_Throw()
        {
            var cacheKey = Guid.NewGuid().ToString();

            _redisPersistence.Setup(c => c.GetCachedValue(cacheKey)).Returns((byte[])default);

            var ex = Assert.Throws<Exception>(() => _target.GetCachedValue<DateTime>(cacheKey));
            Assert.That(ex.Message, Is.EqualTo("Cached value from db is empty"));
        }

        [Test]
        public void WhenCallGetCachedValueAsync_ValueIsEmpty_Throw()
        {
            var cacheKey = Guid.NewGuid().ToString();

            _redisPersistence.Setup(c => c.GetCachedValueAsync(cacheKey)).ReturnsAsync((byte[])default);

            var ex = Assert.ThrowsAsync<Exception>(async () => await  _target.GetCachedValueAsync<DateTime>(cacheKey));
            Assert.That(ex.Message, Is.EqualTo("Cached value from db is empty"));
        }

        [Test]
        [TestCase(true, true)]
        [TestCase(false, false)]
        public void WhenCallContainsKey_ReturnExpectedValue(bool expected, bool isContain)
        {
            var cacheKey = Guid.NewGuid().ToString();

            _redisPersistence.Setup(c => c.ContainsKey(cacheKey)).Returns(isContain);

            var actual = _target.ContainsKey(cacheKey);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        [TestCase(true, true)]
        [TestCase(false, false)]
        public async Task WhenCallContainsKeyAsync_ReturnExpectedValue(bool expected, bool isContain)
        {
            var cacheKey = Guid.NewGuid().ToString();

            _redisPersistence.Setup(c => c.ContainsKeyAsync(cacheKey)).ReturnsAsync(isContain);

            var actual = await _target.ContainsKeyAsync(cacheKey);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void WhenCallDeleteItem_DoesNotThrow()
        {
            var cacheKey = Guid.NewGuid().ToString();

            _redisPersistence.Setup(c => c.DeleteItem(cacheKey));

            Assert.DoesNotThrow(() =>
            {
                _target.DeleteItem(cacheKey);
            });
        }

        [Test]
        public void WhenCallDeleteItemAsync_DoesNotThrow()
        {
            var cacheKey = Guid.NewGuid().ToString();

            _redisPersistence.Setup(c => c.DeleteItemAsync(cacheKey));

            Assert.DoesNotThrowAsync(async () =>
            {
                await _target.DeleteItemAsync(cacheKey);
            });
        }

        [Test]
        [TestCase(true, "dataPayload", "05/05/2002", "05/05/2002", true)]
        [TestCase(true,  null        , "05/05/2002", "05/05/2002", true)]
        [TestCase(false, null        , "1/1/0001", "05/05/2002", false)]
        public void WhenCallTryGet_ReturnExpectedValue(bool expected, string dataPayload, 
            DateTime expectedDate, DateTime dateInCache, bool redisCacheContains)
        {
            var cacheKey = Guid.NewGuid().ToString();
            var dataPayloadByteArray = ToByteArray(dataPayload);

            _serializer.Setup(c => c.Deserialize<DateTime>(dataPayloadByteArray)).Returns(dateInCache);

            _redisPersistence.Setup(c => c.ContainsKey(cacheKey)).Returns(redisCacheContains);

            _redisPersistence.Setup(c => c.GetCachedValue(cacheKey)).Returns(dataPayloadByteArray);

            DateTime actualCacheValue = default;

            var actual = _target.TryGet<DateTime>(cacheKey, out actualCacheValue);

            Assert.AreEqual(actual, expected);
            Assert.AreEqual(actualCacheValue, expectedDate);

        }

        private static byte[] ToByteArray(string dataPayload) 
        {
            if (dataPayload == null)
            {
                return Array.Empty<byte>();
            }

            return Encoding.UTF8.GetBytes(dataPayload);
        }
    }
}
