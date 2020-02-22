using System;
using System.Threading.Tasks;

namespace Vera.Concurrency
{
    public sealed class NullLocker : ILocker
    {
        private NullLocker() { }

        public Task<IAsyncDisposable> Lock(string resource, TimeSpan timeout)
        {
            return Task.FromResult<IAsyncDisposable>(new NullDisposable());
        }

        private class NullDisposable : IAsyncDisposable
        {
            public ValueTask DisposeAsync()
            {
                return new ValueTask(Task.CompletedTask);
            }
        }
    }
}