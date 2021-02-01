using System;
using System.IO;
using System.Threading.Tasks;

namespace Vera.Stores
{
    /// <summary>
    /// Implementation of the blob store that uses the current user' temp directory to write blobs to.
    /// </summary>
    public class TemporaryBlobStore : IBlobStore
    {
        public async Task<string> Store(Guid accountId, Stream data)
        {
            var dir = Path.Join(Path.GetTempPath(), "vera", accountId.ToString());
            var fileName = Guid.NewGuid().ToString();

            await using var fs = File.Create(Path.Join(dir, fileName), 4096, FileOptions.Asynchronous);

            await data.CopyToAsync(fs);

            return fileName;
        }
    }
}