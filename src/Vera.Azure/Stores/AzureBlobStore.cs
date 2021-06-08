using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Vera.Models;
using Vera.Stores;

namespace Vera.Azure.Stores
{
    public class AzureBlobStore : IBlobStore
    {
        private const string accountMeta = "account_id";
        private readonly string _containerName;
        private readonly BlobServiceClient _client;

        public AzureBlobStore(string connectionString, string containerName = "blobs")
        {
            if (string.IsNullOrWhiteSpace(containerName)) throw new NullReferenceException(nameof(containerName));

            _containerName = containerName;
            _client = new BlobServiceClient(connectionString);
        }

        public async Task<string> Store(Guid accountId, Blob blob)
        {
            var container = _client.GetBlobContainerClient(_containerName);
            await container.CreateIfNotExistsAsync();

            var name = Guid.NewGuid().ToString();

            var client = container.GetBlockBlobClient(name);

            await client.UploadAsync(blob.Content,
                new BlobHttpHeaders
                {
                    ContentType = blob.MimeType
                },
                new Dictionary<string, string>
                {
                    { accountMeta, accountId.ToString() }
                });

            return name;
        }

        public async Task<Blob?> Read(Guid accountId, string name)
        {
            var container = _client.GetBlobContainerClient(_containerName);
            
            if (!await container.ExistsAsync())
            {
                return null;
            }
            
            var client = container.GetBlockBlobClient(name);
            
            if (!await client.ExistsAsync())
            {
                return null;
            }

            var response = await client.DownloadAsync();
            if (response.Value.Details.Metadata.TryGetValue(accountMeta, out var meta) && meta == accountId.ToString())
            {
                return new Blob
                {
                    MimeType = response.Value.ContentType,
                    Content = response.Value.Content
                };
            }

            return null;
        }
    }
}
