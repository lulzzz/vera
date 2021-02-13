using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vera.Concurrency
{
    public sealed class InMemoryLocker : ILocker
    {
        private readonly SemaphoreSlim _semaphore;

        public InMemoryLocker()
        {
            _semaphore = new SemaphoreSlim(1, 1);
        }

        public async Task<IAsyncDisposable> Lock(string resource, TimeSpan timeout)
        {
            await _semaphore.WaitAsync(timeout);

            return new InMemoryLockDisposable(_semaphore);
        }

        private class InMemoryLockDisposable : IAsyncDisposable
        {
            private readonly SemaphoreSlim _semaphore;

            public InMemoryLockDisposable(SemaphoreSlim semaphore)
            {
                _semaphore = semaphore;
            }

            public ValueTask DisposeAsync()
            {
                _semaphore.Release();
                return new ValueTask();
            }
        }
    }
}