using System;
using System.Threading.Tasks;

namespace Rushan.Foundation.Redis.Tests.Dummy
{
    public class DummyClass : IEquatable<DummyClass>, IDummyInterface
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Guid Key { get; set; }

        public DateTime Value { get; set; }

        public bool Equals(DummyClass other)
        {
            return this.Id == other.Id &&
                this.Name == other.Name &&
                this.Key == other.Key &&
                this.Value == other.Value;
        }

        public DateTime DummyMetod()
        {
            return DateTime.Now;
        }

        public async Task<DateTime> DummyMetodAsync()
        {
            return await Task.FromResult(DateTime.Now);
        }
    }
}
