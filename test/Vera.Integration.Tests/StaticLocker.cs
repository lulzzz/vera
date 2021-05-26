using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Vera.Concurrency;

namespace Vera.Integration.Tests
{
    public class StaticLocker : ILocker
    {
        private static readonly ILocker Locker = new InMemoryLocker();

        public Task<IAsyncDisposable> Lock(string resource, TimeSpan timeout)
        {
            return Locker.Lock(resource, timeout);
        }
    }
}