using System.Linq;
using Vera.Models;

namespace Vera.Documents
{
    public interface IThermalReceiptContextFactory
    {
        ThermalReceiptContext Create(Account account, Invoice invoice);
    }

    public class ThermalReceiptContextFactory
    {
        public ThermalReceiptContext Create(Account account, Invoice invoice)
        {
            return new ThermalReceiptContext
            {
                Account = account,
                Invoice = invoice,
                Totals = CalculateInvoiceTotals(invoice),
                // TODO(kevin): get other properties from the account config
            };
        }

        private Totals CalculateInvoiceTotals(Invoice invoice)
        {
            var totals = new Totals();

            foreach (var line in invoice.Lines)
            {
                totals.Amount += line.Amount;
                totals.AmountInTax += line.AmountInTax;
            }

            totals.Taxes = invoice.Lines
                .GroupBy(l => l.TaxRate)
                .Select(g =>
                {
                    var baseAmount = g.Sum(l => l.Amount);
                    return new TaxTotal
                    {
                        Rate = g.Key,
                        Base = baseAmount,
                        Amount = baseAmount * (g.Key - 1)
                    };
                })
                .ToList();

            return totals;
        }
    }
}