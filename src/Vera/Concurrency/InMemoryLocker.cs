using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Vera.Concurrency
{
    public sealed class InMemoryLocker : ILocker
    {
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks;
        private readonly SemaphoreSlim _semaphore;

        public InMemoryLocker()
        {
            _locks = new ConcurrentDictionary<string, SemaphoreSlim>();
            _semaphore = new SemaphoreSlim(1, 1);
        }
        
        public async Task<IAsyncDisposable> Lock(string resource, TimeSpan timeout)
        {
            await _semaphore.WaitAsync(timeout);

            return new InMemoryLockDisposable(_semaphore);
        }

        private class InMemoryLockDisposable : IAsyncDisposable
        {
            private readonly SemaphoreSlim _mutex;

            public InMemoryLockDisposable(SemaphoreSlim mutex)
            {
                _mutex = mutex;
            }

            public ValueTask DisposeAsync()
            {
                _mutex.Release();
                return new ValueTask();
            }
        }
    }
}