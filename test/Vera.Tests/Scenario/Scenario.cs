using System;
using System.Threading.Tasks;
using Vera.Invoices;
using Vera.Models;

namespace Vera.Tests.Scenario
{
    public abstract class Scenario
    {
        private readonly InvoiceTotalsCalculator _calculator;

        protected Scenario()
        {
            _calculator = new InvoiceTotalsCalculator();
        }

        public ScenarioResult Execute()
        {
            var invoice = Create();
            invoice.Remark = GetType().Name;
            invoice.Totals = _calculator.Calculate(invoice);
            
            return new(invoice);
        }

        protected abstract Invoice Create();

        public Guid AccountId { get; set; }
        public string SupplierSystemId { get; set; }
    }

    public class ScenarioResult
    {
        public ScenarioResult(Models.Invoice invoice)
        {
            Invoice = invoice;
        }
        
        public Models.Invoice Invoice { get; set; }
    }
}