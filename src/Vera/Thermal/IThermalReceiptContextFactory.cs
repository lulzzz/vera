using Vera.Invoices;
using Vera.Models;

namespace Vera.Thermal
{
    public interface IThermalReceiptContextFactory
    {
        ThermalReceiptContext Create(Account account, Invoice invoice);
    }

    public class ThermalReceiptContextFactory
    {
        private readonly InvoiceTotalsCalculator _calculator;

        public ThermalReceiptContextFactory()
        {
            _calculator = new InvoiceTotalsCalculator();
        }

        public ThermalReceiptContext Create(Account account, Invoice invoice)
        {
            return new()
            {
                Account = account,
                Invoice = invoice

                // TODO(kevin): set the other properties (header/footer/etc.)
            };
        }
    }
}