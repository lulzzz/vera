using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Vera.Models;
using Vera.Stores;

namespace Vera.Invoices
{
    public class InvoiceOpenPeriodHandler : InvoiceHandler
    {
        private readonly IPeriodStore _periodStore;

        public InvoiceOpenPeriodHandler(IPeriodStore periodStore)
        {
            _periodStore = periodStore;
        }

        public override async Task Handle(Invoice invoice)
        {
            var period = await _periodStore.GetOpenPeriodForSupplier(invoice.Supplier.Id);
            
            if (period == null || period.IsClosed)
            {
                throw new ValidationException("An open period is required");
            }

            invoice.PeriodId = period.Id;
            
            await base.Handle(invoice);
        }
    }
}