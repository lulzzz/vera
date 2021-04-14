using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Vera.Dependencies.Handlers;
using Vera.Models;
using Vera.Stores;

namespace Vera.Invoices
{
    public class InvoiceSupplierHandler : HandlerChain<Invoice>
    {
        private readonly ISupplierStore _supplierStore;

        public InvoiceSupplierHandler(ISupplierStore supplierStore)
        {
            _supplierStore = supplierStore;
        }

        public override async Task Handle(Invoice invoice)
        {
            var supplierSystemId = invoice.Supplier?.SystemId;

            if (string.IsNullOrEmpty(supplierSystemId))
            {
                throw new ValidationException("Missing supplier");
            }
            
            var supplier = await _supplierStore.Get(invoice.AccountId, supplierSystemId);
            
            if (supplier == null)
            {
                throw new ValidationException($"Unknown supplier '{supplierSystemId}'");
            }

            invoice.Supplier = supplier;
            
            await base.Handle(invoice);
        }
    }
}