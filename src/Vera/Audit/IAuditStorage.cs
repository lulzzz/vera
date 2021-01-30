using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;

namespace Vera.Audit
{
    // TODO: generic file storage?
    public interface IAuditStorage
    {
        Task Store(Stream s);
    }

    public class AzureAuditStorage
    {
        private readonly string _containerName;
        private readonly BlobServiceClient _client;

        public AzureAuditStorage(string connectionString, string containerName = "audits")
        {
            if (string.IsNullOrWhiteSpace(containerName)) throw new NullReferenceException(nameof(containerName));

            _containerName = containerName;
            _client = new BlobServiceClient(connectionString);
        }

        public async Task Store(Stream s)
        {
            var container = _client.GetBlobContainerClient(_containerName);
            await container.CreateIfNotExistsAsync();
        }
    }
}