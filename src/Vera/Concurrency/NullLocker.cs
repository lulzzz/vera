using System;
using System.Threading.Tasks;

namespace Vera.Concurrency
{
    public sealed class NullLocker : ILocker
    {
        public Task<IAsyncDisposable> Lock(string resource, TimeSpan timeout)
        {
            return Task.FromResult<IAsyncDisposable>(new NullDisposable());
        }

        private class NullDisposable : IAsyncDisposable
        {
            public ValueTask DisposeAsync() => new(Task.CompletedTask);
        }
    }
}