using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Vera.Concurrency
{
    public sealed class InMemoryLocker : ILocker
    {
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores;

        public InMemoryLocker()
        {
            _semaphores = new();
        }

        public async Task<IAsyncDisposable> Lock(string resource, TimeSpan timeout)
        {
            var semaphore = _semaphores.GetOrAdd(resource, _ => new SemaphoreSlim(1, 1));
            
            if (!await semaphore.WaitAsync(timeout))
            {
                throw new TimeoutException($"failed to acquire lock in {timeout}");
            }

            return new InMemoryLockDisposable(semaphore);
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