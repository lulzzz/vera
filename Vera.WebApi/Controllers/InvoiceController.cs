using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Vera.WebApi.Controllers
{
    [ApiController]
    [Route("invoice")]
    public class InvoiceController : ControllerBase
    {
        private readonly ILogger<InvoiceController> _logger;

        public InvoiceController(ILogger<InvoiceController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // TODO: get invoice storage
            // TODO: get component factory for provider
            // TODO: create InvoiceFacade

            return Ok(new
            {
                Id = 1
            });
        }
    }
}
