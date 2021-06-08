using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vera.Host.Security;
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

        [HttpGet("/download/audit/{name}")]
        public async Task<IActionResult> Download(string name)
        {
            if (HttpContext.Request.Headers.TryGetValue(MetadataKeys.AccountId, out var accountIdValue) &&
                Guid.TryParse(accountIdValue, out var accountId))
            {
                var blob = await _blobStore.Read(accountId, name);

                if (blob != null)
                {
                    return File(blob.Content, blob.MimeType);
                }

                return NotFound(name);
            }

            return Forbid(name);
        }
    }
}
