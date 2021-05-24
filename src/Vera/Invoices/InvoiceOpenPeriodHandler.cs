using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Vera.Dependencies.Handlers;
using Vera.Models;
using Vera.Stores;

namespace Vera.Invoices
{
    public class InvoiceOpenPeriodHandler : HandlerChain<Invoice>
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

            var register = period.Registers.FirstOrDefault(r => r.RegisterId.ToString() == invoice.RegisterId);
            if (register == null)
            {
                throw new ValidationException("An open register is required");
            }
            
            await base.Handle(invoice);
        }
    }
}