using System.Linq;
using Vera.Models;

namespace Vera.Invoices
{
    public interface IInvoiceTotalCalculator
    {
        Totals Calculate(Invoice invoice);
    }

    public class InvoiceTotalCalculator
    {
        public Totals Calculate(Invoice invoice)
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