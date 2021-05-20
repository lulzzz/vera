using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vera.Extensions;
using Vera.Invoices;
using Xunit;

namespace Vera.Tests.Invoices.Handlers
{
    public class InvoiceTotalsHandlerTests
    {
        [Fact]
        public async Task Should_calculate_totals_and_invoke_next()
        {
            var line1 = new Models.InvoiceLine
            {
                UnitPrice = 1.99m,
                Quantity = 1,
                Taxes = new Models.Taxes
                {
                    Rate = 1.23m,
                    Category = Models.TaxesCategory.High
                }
            };
            var line2 = new Models.InvoiceLine
            {
                UnitPrice = 2.49m,
                Quantity = 1,
                Taxes = new Models.Taxes
                {
                    Rate = 1.23m,
                    Category = Models.TaxesCategory.High
                }
            };
            var invoice = new Models.Invoice
            {
                Lines = new List<Models.InvoiceLine>
                {
                    line1,
                    line2
                }
            };
            var totalsCalculatorHandler = new InvoiceTotalsHandler();
            var mockHandler = new InvoiceHandlersHelper().MockInvoiceHandler;
            totalsCalculatorHandler.WithNext(mockHandler.Object);

            await totalsCalculatorHandler.Handle(invoice);

            Assert.Equal(invoice.Totals.Net, GetNetTotal(invoice.Lines));
            Assert.Equal(invoice.Totals.Gross, GetGrossTotal(invoice.Lines));

            mockHandler.Verify(h => h.Handle(invoice));
        }

        private decimal GetNetTotal(IEnumerable<Models.InvoiceLine> lines) => 
            lines.Aggregate(0m, (netTotal, line) => netTotal + (line.Quantity * line.UnitPrice).RoundKeepSign(2));

        private decimal GetGrossTotal(IEnumerable<Models.InvoiceLine> lines) =>
            lines.Aggregate(0m, (grossTotal, line) => grossTotal + (line.Quantity * line.UnitPrice * line.Taxes.Rate).RoundKeepSign(2));
    }
}
