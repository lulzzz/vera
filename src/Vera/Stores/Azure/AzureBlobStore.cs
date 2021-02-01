using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;

namespace Vera.Stores.Azure
{
    public class AzureBlobStore : IBlobStore
    {
        private readonly string _containerName;
        private readonly BlobServiceClient _client;

        public AzureBlobStore(string connectionString, string containerName)
        {
            if (string.IsNullOrWhiteSpace(containerName)) throw new NullReferenceException(nameof(containerName));

            _containerName = containerName;
            _client = new BlobServiceClient(connectionString);
        }

        public async Task<string> Store(Guid accountId, Stream data)
        {
            var container = _client.GetBlobContainerClient(_containerName);
            await container.CreateIfNotExistsAsync();

            var name = Guid.NewGuid().ToString();

            var client = container.GetBlockBlobClient(name);

            await client.UploadAsync(data);

            return name;
        }
    }
}