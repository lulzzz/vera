using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Vera.Models;
using Vera.Stores;

namespace Vera.Invoices
{
    public class InvoiceSupplierHandler : InvoiceHandler
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
            
            var supplier = await _supplierStore.GetBySystemId(supplierSystemId);
            
            if (supplier == null)
            {
                throw new ValidationException($"Unknown supplier '{supplierSystemId}'");
            }

            invoice.Supplier = supplier;
            
            await base.Handle(invoice);
        }
    }
}