using Rushan.Foundation.Redis.Serialization;
using AutoFixture;
using NUnit.Framework;
using Rushan.Foundation.Redis.Tests.Dummy;

namespace Rushan.Foundation.Redis.Tests.Serializer
{
    [TestFixture]
    [Category("Unit")]
    public class JsonSerializerTest
    {
        private ISerializer _target;

        private readonly Fixture _fixture;

        public JsonSerializerTest()
        {
            _fixture = new Fixture();
        }

        [OneTimeSetUp]
        public void SetUp()
        {
            _target = new JsonSerializer();
        }

        [Test]
        [Repeat(10)]
        public void WhenCallSerializeAndDeserialize_ReturnEqualResult()
        {
            var source = _fixture.Create<DummyClass>();

            var serializedItem = _target.Serialize(source);
            var actual = _target.Deserialize<DummyClass>(serializedItem);

            Assert.AreEqual(source, actual);
        }

        [Test]

        public void WhenCallSerializeAndDeserializeNullableValue_ReturnEqualResult()
        {
            var source = (DummyClass)null;

            var serializedItem = _target.Serialize(source);
            var actual = _target.Deserialize<DummyClass>(serializedItem);

            Assert.AreEqual(source, actual);
        }
    }
}
