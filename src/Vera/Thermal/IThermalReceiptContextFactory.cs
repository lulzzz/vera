using System.Linq;
using System.Threading.Tasks;
using Vera.Models;
using Vera.Stores;

namespace Vera.Thermal
{
    public interface IThermalReceiptContextFactory
    {
        Task<ThermalReceiptContext> Create(Account account, Invoice invoice);
    }

    public class ThermalReceiptContextFactory
    {
        private readonly IPrintAuditTrailStore _printAuditTrailStore;

        public ThermalReceiptContextFactory(IPrintAuditTrailStore printAuditTrailStore)
        {
            _printAuditTrailStore = printAuditTrailStore;
        }

        public async Task<ThermalReceiptContext> Create(Account account, Invoice invoice)
        {
            var prints = await _printAuditTrailStore.GetByInvoice(invoice.Id);

            return new()
            {
                Account = account,
                Invoice = invoice,
                Prints = prints,
                Original = !prints.Any(x => x.Success)

                // TODO(kevin): set the other properties (header/footer/etc.)
            };
        }
    }
}