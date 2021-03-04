using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vera.Stores;

namespace Vera.Host.Controllers
{
    [Authorize]
    public class DownloadController : ControllerBase
    {
        private readonly IBlobStore _blobStore;

        public DownloadController(IBlobStore blobStore)
        {
            _blobStore = blobStore;
        }

        [HttpGet("/download/audit/{accountId}/{name}")]
        public async Task<IActionResult> Download(Guid accountId, string name)
        {
            // TODO(kevin): check that account is allowed to access
            
            var blob = await _blobStore.Read(accountId, name);

            if (blob == null) return NotFound(name);
            
            return File(blob.Content, blob.MimeType);
        }
    }
}