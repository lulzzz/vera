using System;
using System.IO;
using System.Threading.Tasks;
using Vera.Models;

namespace Vera.Stores
{
    /// <summary>
    /// Implementation of the blob store that uses the current user' temp directory to write blobs to.
    /// </summary>
    public class TemporaryBlobStore : IBlobStore
    {
        public async Task<string> Store(Guid accountId, Blob blob)
        {
            var dir = GetDirectoryName(accountId);

            Directory.CreateDirectory(dir);

            var fileName = Guid.NewGuid().ToString();

            await using var fs = File.Create(Path.Join(dir, fileName), 4096, FileOptions.Asynchronous);
            await blob.Content.CopyToAsync(fs);

            await File.WriteAllTextAsync(Path.Join(dir, $"{fileName}_mime"), blob.MimeType);
            
            return fileName;
        }

        public async Task<Blob?> Read(Guid accountId, string name)
        {
            var dir = GetDirectoryName(accountId);
            var contentFile = Path.Join(dir, name);

            if (!File.Exists(contentFile))
            {
                return null;
            }
            
            var mimeType = File.ReadAllTextAsync(Path.Join(dir, $"{name}_mime"));
            var content = File.OpenRead(contentFile);

            return new Blob
            {
                Content = content,
                MimeType = await mimeType
            };
        }

        private static string GetDirectoryName(Guid accountId) =>
            Path.Join(Path.GetTempPath(), "vera", accountId.ToString());
    }
}