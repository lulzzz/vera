using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vera.Bootstrap;
using Vera.Invoices;
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

        [HttpPost]
        public async Task<IActionResult> Index(Models.Invoice invoice)
        {
            var accountConfig = new AccountConfig
            {
                Name = "PT"
            };

            var factory = _componentFactoryCollection.Get(accountConfig);

            var facade = new InvoiceFacade(
                _invoiceStore,
                factory.CreateLocker(),
                factory.CreateInvoiceBucketGenerator(),
                factory.CreateInvoiceNumberGenerator(),
                factory.CreatePackageSigner()
            );

            var result = await facade.Process(invoice.ToModel());

            return Ok(new
            {
                Number = result.Number,
                Sequence = result.Sequence,
                RawSignature = result.RawSignature,
                Signature = result.Signature
            });
        }
    }
}
