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

        public InMemoryLocker()
        {
            _locks = new ConcurrentDictionary<string, SemaphoreSlim>();
        }
        
        public async Task<IAsyncDisposable> Lock(string resource, TimeSpan timeout)
        {
            var mutex = _locks.GetOrAdd(resource, _ => new SemaphoreSlim(1, 1));
            await mutex.WaitAsync(timeout);

            return new InMemoryLockDisposable(mutex);
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