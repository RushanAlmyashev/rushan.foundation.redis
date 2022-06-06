using Foundation.Redis.Logger;
using Foundation.Redis.Persistences;
using Foundation.Redis.Serialization;
using Foundation.Redis.Tests.Dummy;
using AutoFixture;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Foundation.Redis.Tests.Persistences
{
    [TestFixture]
    [Category("Integration")]
    [Ignore("Integration")]
    public class RedisCachePersistenceTests
    {
        private IRedisPersistence _target;
        
        private readonly Fixture _fixture;

        private ILogger _logger;
        private ISerializer _serializer;
        private string _connectionString;

        public RedisCachePersistenceTests()
        {
            _fixture = new Fixture();
        }

        [OneTimeSetUp]
        public void SetUp()
        {
            _logger = new EmptyLogger();
            _serializer = new JsonSerializer();
            _connectionString = "Yours db redis connection string";

            _target = new RedisPersistence(_connectionString, _logger);
        }

        [Test]
        public void WhenSetAndGetValue_ReturnExpectedResult()
        {
            var key = Guid.NewGuid().ToString();
            var value = _fixture.Create<DummyClass>();

            var byteValue = _serializer.Serialize(value);
            _target.SetCachedValue(key, byteValue, TimeSpan.FromSeconds(10));
            var byteActual = _target.GetCachedValue(key);
            var actual = _serializer.Deserialize<DummyClass>(byteActual);

            Assert.AreEqual(actual, value);
        }

        [Test]
        public async Task WhenSetAndGetValueAsync_ReturnExpectedResult()
        {
            var key = Guid.NewGuid().ToString();
            var value = _fixture.Create<DummyClass>();

            var byteValue = _serializer.Serialize(value);
            await _target.SetCachedValueAsync(key, byteValue, TimeSpan.FromSeconds(10));
            var byteActual = await _target.GetCachedValueAsync(key);
            var actual = _serializer.Deserialize<DummyClass>(byteActual);

            Assert.AreEqual(actual, value);
        }

        [Test]
        public void WhenCallContainsKey_ReturnExpectedResult()
        {
            var value = _fixture.Create<DummyClass>();
            var keyOne = Guid.NewGuid().ToString();
            var keyTwo = Guid.NewGuid().ToString();

            var byteValue = _serializer.Serialize(value);
            _target.SetCachedValue(keyOne, byteValue, TimeSpan.FromSeconds(10));

            var actualOne = _target.ContainsKey(keyOne);
            var actualTwo = _target.ContainsKey(keyTwo);

            Assert.AreEqual(actualOne, true);
            Assert.AreEqual(actualTwo, false);
        }

        [Test]
        public async Task WhenCallContainsKeyAsync_ReturnExpectedResult()
        {
            var value = _fixture.Create<DummyClass>();
            var keyOne = Guid.NewGuid().ToString();
            var keyTwo = Guid.NewGuid().ToString();

            var byteValue = _serializer.Serialize(value);
            await _target.SetCachedValueAsync(keyOne, byteValue, TimeSpan.FromSeconds(10));

            var actualOneTask = _target.ContainsKeyAsync(keyOne);
            var actualTwoTask = _target.ContainsKeyAsync(keyTwo);

            var actualTwo = await actualTwoTask;
            var actualOne = await actualOneTask;

            Assert.AreEqual(actualOne, true);
            Assert.AreEqual(actualTwo, false);
        }

        [Test]
        public void WhenCallDelete_ReturnExpectedResult()
        {
            var value = _fixture.Create<DummyClass>();
            var key = Guid.NewGuid().ToString();

            var byteValue = _serializer.Serialize(value);
            _target.SetCachedValue(key, byteValue, TimeSpan.FromSeconds(10));
            _target.DeleteItem(key);

            var actual = _target.ContainsKey(key);

            Assert.AreEqual(actual, false);
        }

        [Test]
        public async Task WhenCallDeleteAsync_ReturnExpectedResult()
        {
            var value = _fixture.Create<DummyClass>();
            var key = Guid.NewGuid().ToString();

            var byteValue = _serializer.Serialize(value);
            await _target.SetCachedValueAsync(key, byteValue, TimeSpan.FromSeconds(10));
            await _target.DeleteItemAsync(key);

            var actual = await _target.ContainsKeyAsync(key);

            Assert.AreEqual(actual, false);
        }

        [Test]
        public void WhenCallLockTake_NoneCanTakeIt()
        {
            var valueOne = Guid.NewGuid().ToString();
            var valueTwo = Guid.NewGuid().ToString();
            var key = Guid.NewGuid().ToString();

            var actualOne = _target.LockTake(key, valueOne, TimeSpan.FromSeconds(10));
            var actualTwo = _target.LockTake(key, valueTwo, TimeSpan.FromSeconds(10));

            Assert.AreEqual(actualOne, true);
            Assert.AreEqual(actualTwo, false);
        }

        [Test]
        public void WhenCallLockRelease_ItemReleased()
        {
            var key = Guid.NewGuid().ToString();
            var value = Guid.NewGuid().ToString();

            var actualOne = _target.LockTake(key, value, TimeSpan.FromSeconds(10));
            var actualTwo = _target.LockTake(key, value, TimeSpan.FromSeconds(10));

            _target.LockRelease(key, value);

            var actualThree = _target.LockTake(key, value, TimeSpan.FromSeconds(10));

            Assert.AreEqual(actualOne, true);
            Assert.AreEqual(actualTwo, false);
            Assert.AreEqual(actualThree, true);
        }

        [Test]
        public void WhenCallKeyTimeToLive_ReturnExpectedValue()
        {
            var key = Guid.NewGuid().ToString();
            var value = Guid.NewGuid().ToString();

            var byteValue = _serializer.Serialize(value);

            _target.SetCachedValue(key, byteValue, TimeSpan.FromSeconds(10));
            var actualThree = _target.KeyTimeToLive(key);

            Assert.GreaterOrEqual(TimeSpan.FromSeconds(10), actualThree);
            Assert.LessOrEqual(TimeSpan.FromSeconds(0), actualThree);
        }

        [Test]
        public async Task WhenCallKeyTimeToLiveAsync_ReturnExpectedValue()
        {
            var key = Guid.NewGuid().ToString();
            var value = Guid.NewGuid().ToString();

            var byteValue = _serializer.Serialize(value);

            await _target.SetCachedValueAsync(key, byteValue, TimeSpan.FromSeconds(10));
            var actualThree = await _target.KeyTimeToLiveAsync(key);

            Assert.GreaterOrEqual(TimeSpan.FromSeconds(10), actualThree);
            Assert.LessOrEqual(TimeSpan.FromSeconds(0), actualThree);
        }
    }
}
