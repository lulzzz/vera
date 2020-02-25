using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vera.Bootstrap;
using Vera.Stores;

namespace Vera.WebApi.Controllers
{
    [ApiController]
    [Route("invoice")]
    public class InvoiceController : ControllerBase
    {
        private readonly ILogger<InvoiceController> _logger;
        private readonly IInvoiceStore _invoiceStore;
        private readonly IComponentFactoryCollection _componentFactoryCollection;

        public InvoiceController(
            ILogger<InvoiceController> logger,
            IInvoiceStore invoiceStore,
            IComponentFactoryCollection componentFactoryCollection
        )
        {
            _logger = logger;
            _invoiceStore = invoiceStore;
            _componentFactoryCollection = componentFactoryCollection;
        }

        public IActionResult Index()
        {
            // var facade = registry.Get(configuration)
            // facade.Process(..)

            // factory that creates the component factory
            // factory that creates the factory will be based on the account config
            // platform config: vera stuff (typed)
            //   - cosmos
            //   - blob
            // account config: provider stuff (untyped?)
            //   - keys (RSA, AES, etc.)
            //   - ATrust endpoint/credentials


            // Vera.Bootstrap
            // reference to all the providers
            // fulfill each provider with their dependencies
            // reference bootstrap
            // bootstrap contains register with all the providers
            // bootstrap registery > Get(account config) > facade
            // profit?

            // TODO: get invoice storage
            // TODO: get component factory for provider
            // TODO: create InvoiceFacade

            var factory = _componentFactoryCollection.Get(new AccountConfig
            {
                Name = "PT"
            });

            var facade = new InvoiceFacade(
                _invoiceStore,
                factory.CreateLocker(),
                factory.CreateInvoiceBucketGenerator(),
                factory.CreateInvoiceNumberGenerator(),
                factory.CreatePackageSigner()
            );

            return Ok(new
            {
                Id = 1
            });
        }
    }
}
