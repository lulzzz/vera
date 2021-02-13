using System;
using System.Threading;
using System.Threading.Tasks;
using Vera.Concurrency;

namespace Vera.Integration.Tests
{
    public class StaticLocker : ILocker
    {
        private static readonly SemaphoreSlim Semaphore = new(1, 1);

        public async Task<IAsyncDisposable> Lock(string resource, TimeSpan timeout)
        {
            await Semaphore.WaitAsync(timeout);
            return new StaticLockDisposable(Semaphore);
        }
        
        private class StaticLockDisposable : IAsyncDisposable
        {
            private readonly SemaphoreSlim _semaphore;

            public StaticLockDisposable(SemaphoreSlim semaphore)
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