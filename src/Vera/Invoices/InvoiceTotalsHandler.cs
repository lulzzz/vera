using System.Threading.Tasks;
using Vera.Dependencies.Handlers;
using Vera.Models;

namespace Vera.Invoices
{
    public class InvoiceTotalsHandler : HandlerChain<Invoice>
    {
        private readonly InvoiceTotalsCalculator _calculator;

        public InvoiceTotalsHandler()
        {
            _calculator = new InvoiceTotalsCalculator();
        }

        public override Task Handle(Invoice invoice)
        {
            invoice.Totals = _calculator.Calculate(invoice);
            
            return base.Handle(invoice);
        }
    }
}