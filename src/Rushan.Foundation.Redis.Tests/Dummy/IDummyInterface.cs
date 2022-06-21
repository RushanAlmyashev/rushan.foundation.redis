using System;
using System.Threading.Tasks;

namespace Rushan.Foundation.Redis.Tests.Dummy
{
    public interface IDummyInterface
    {
        DateTime DummyMetod();
        Task<DateTime> DummyMetodAsync();
    }
}