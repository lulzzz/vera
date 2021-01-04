using System.Linq;
using Vera.Models;

namespace Vera.Thermal
{
    public interface IThermalReceiptContextFactory
    {
        ThermalReceiptContext Create(Account account, Invoice invoice);
    }

    public class ThermalReceiptContextFactory
    {
        public ThermalReceiptContext Create(Account account, Invoice invoice)
        {
            return new()
            {
                Account = account,
                Invoice = invoice,
                Totals = CalculateInvoiceTotals(invoice)
            };
        }

        private Totals CalculateInvoiceTotals(Invoice invoice)
        {
            var totals = new Totals();

            foreach (var line in invoice.Lines)
            {
                totals.Amount += line.Gross;
                totals.AmountInTax += line.Net;
            }

            totals.Taxes = invoice.Lines
                .GroupBy(l => l.Taxes.Rate)
                .Select(g =>
                {
                    var baseAmount = g.Sum(l => l.Gross);
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