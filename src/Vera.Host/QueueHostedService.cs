using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Vera.Host
{
    public class QueueHostedService : BackgroundService
    {
        private readonly ILogger<QueueHostedService> _logger;
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;

        public QueueHostedService(
            ILogger<QueueHostedService> logger,
            IBackgroundTaskQueue backgroundTaskQueue
        )
        {
            _logger = logger;
            _backgroundTaskQueue = backgroundTaskQueue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var work = await _backgroundTaskQueue.DequeueAsync(stoppingToken);

                try
                {
                    await work(stoppingToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "failed to run task");
                }
            }
        }
    }
}