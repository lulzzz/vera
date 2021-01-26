using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Hosting;
using Vera.Models;
using Vera.Stores;

namespace Vera.WebApi.Background
{
    public class InvoiceViewHostedService : IHostedService
    {
        private readonly Container _container;
        private ChangeFeedProcessor _changeFeedProcessor;

        public InvoiceViewHostedService(Container container)
        {
            _container = container;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            const string leaseContainerName = "leases";

            var leaseContainer = await _container.Database.CreateContainerIfNotExistsAsync(
                leaseContainerName,
                "/id",
                cancellationToken: cancellationToken
            );

            _changeFeedProcessor = _container
                .GetChangeFeedProcessorBuilder<ChainableDocument<Invoice>>("BuildInvoiceView", HandleChangesAsync)
                .WithInstanceName("vera-1") // TODO(kevin): set to container name and fallback for local?
                .WithLeaseContainer(leaseContainer)
                .Build();

            await _changeFeedProcessor.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_changeFeedProcessor != null)
            {
                await _changeFeedProcessor.StopAsync();
            }
        }

        private async Task HandleChangesAsync(IReadOnlyCollection<ChainableDocument<Invoice>> changes, CancellationToken cancellationToken)
        {
            foreach (var change in changes)
            {
                if (change.Value == null)
                {
                    // TODO(kevin): is this the best way to solve the problem for unrelated changes?
                    // unrelated being anything else than the chainable documents
                    continue;
                }

                var partitionKeyValue = change.Value.AccountId + "-" + change.Value.Number;

                // TODO(kevin): leaky abstraction? :-(
                await _container.CreateItemAsync(new CosmosInvoiceStore.InvoiceDocument
                {
                    Id = Guid.NewGuid().ToString(),
                    Invoice = change.Value,
                    PartitionKey = partitionKeyValue
                }, new PartitionKey(partitionKeyValue), cancellationToken: cancellationToken);
            }
        }
    }
}