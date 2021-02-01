using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Vera.WebApi
{
    public interface IBackgroundTaskQueue
    {
        void Queue(Func<CancellationToken, Task> work);

        Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
    }

    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly ConcurrentQueue<Func<CancellationToken, Task>> _workItems = new();
        private readonly SemaphoreSlim _signal = new(0);

        public void Queue(Func<CancellationToken, Task> work)
        {
            if (work == null)
            {
                throw new ArgumentNullException(nameof(work));
            }

            _workItems.Enqueue(work);
            _signal.Release();
        }

        public async Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);

            _workItems.TryDequeue(out var workItem);

            return workItem;
        }
    }
}