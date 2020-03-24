using System;
using System.Net;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;

namespace Vera.Concurrency
{
    public sealed class AzureBlobLocker : ILocker
    {
        private readonly string _containerName;

        private readonly BlobServiceClient _client;
        private readonly Random _random;

        public AzureBlobLocker(string connectionString, string containerName = "locks")
        {
            if (string.IsNullOrEmpty(containerName)) throw new NullReferenceException(nameof(containerName));

            _containerName = containerName;

            _client = new BlobServiceClient(connectionString);
            _random = new Random();
        }

        public async Task<IAsyncDisposable> Lock(string resource, TimeSpan timeout)
        {
            var container = _client.GetBlobContainerClient(_containerName);
            await container.CreateIfNotExistsAsync();
            
            // Create empty blob with resource name to acquire the lease on
            var append = container.GetAppendBlobClient(resource);

            try
            {
                await append.CreateIfNotExistsAsync();
            }
            catch (RequestFailedException e)
            {
                // 412 indicates that there is currently a lease, anything else is an unexpected error
                if (e.Status != (int) HttpStatusCode.PreconditionFailed)
                {
                    throw;
                }
            }

            var lease = append.GetBlobLeaseClient();
            var gotLease = false;

            var delta = TimeSpan.FromMilliseconds(0);

            while (delta < timeout)
            {
                try
                {
                    await lease.AcquireAsync(TimeSpan.FromSeconds(30));

                    gotLease = true;

                    break;
                }
                catch (RequestFailedException e)
                {
                    if (e.Status != (int) HttpStatusCode.Conflict)
                    {
                        // Some other error than: "the lease is already in use"
                        throw;
                    }
                }

                var delay = TimeSpan.FromMilliseconds(_random.Next(50, 150));

                // Wait for lease to become available
                await Task.Delay(delay);

                delta = delta.Add(delay);
            }

            if (!gotLease)
            {
                throw new TimeoutException($"Timeout reached while trying to acquire the lock ({timeout})");
            }

            return new Lease(lease);
        }

        private sealed class Lease : IAsyncDisposable
        {
            private readonly BlobLeaseClient _lease;

            public Lease(BlobLeaseClient lease)
            {
                _lease = lease;
            }

            public ValueTask DisposeAsync()
            {
                return new ValueTask(_lease.ReleaseAsync());
            }
        }
    }
}