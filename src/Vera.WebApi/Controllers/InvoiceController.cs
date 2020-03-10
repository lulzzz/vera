using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vera.Bootstrap;
using Vera.Invoices;
using Vera.Security;
using Vera.Stores;

namespace Vera.WebApi.Controllers
{
    [ApiController]
    [Route("invoice")]
    [Authorize]
    public class InvoiceController : ControllerBase
    {
        private readonly ICompanyStore _companyStore;
        private readonly IInvoiceStore _invoiceStore;
        private readonly IComponentFactoryCollection _componentFactoryCollection;

        public InvoiceController(
            ICompanyStore companyStore,
            IInvoiceStore invoiceStore,
            IComponentFactoryCollection componentFactoryCollection
        )
        {
            _companyStore = companyStore;
            _invoiceStore = invoiceStore;
            _componentFactoryCollection = componentFactoryCollection;
        }

        [HttpPost]
        public async Task<IActionResult> Index(Models.Invoice invoice)
        {
            var company = await _companyStore.GetByName(User.FindFirstValue(ClaimTypes.CompanyName));
            var account = company.Accounts.FirstOrDefault(a => a.Id == invoice.Account);

            if (account == null)
            {
                // Not allowed to create an invoice for this account because it does not belong to the company
                // to which the user has rights
                return Unauthorized();
            }

            var factory = _componentFactoryCollection.Get(account);

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
