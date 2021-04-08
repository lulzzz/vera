using System;
using System.Linq;
using Vera.Extensions;
using Vera.Models;

namespace Vera.Invoices
{
    public class InvoiceTotalsCalculator
    {
        // TODO(kevin): do the rounding to 2 decimals here already?
        public Totals Calculate(Invoice invoice)
        {
            var totals = new Totals();
            var table = totals.Taxes;

            foreach (var line in invoice.Lines)
            {
                var entry = line.Taxes.Category switch
                {
                    TaxesCategory.High => table.High ??= new TaxTable.Entry(line.Taxes.Rate),
                    TaxesCategory.Low => table.Low ??= new TaxTable.Entry(line.Taxes.Rate),
                    TaxesCategory.Zero => table.Zero ??= new TaxTable.Entry(line.Taxes.Rate),
                    TaxesCategory.Exempt => table.Exempt ??= new TaxTable.Entry(line.Taxes.Rate),
                    TaxesCategory.Intermediate => table.Intermediate ??= new TaxTable.Entry(line.Taxes.Rate),
                    _ => throw new ArgumentOutOfRangeException(nameof(line.Taxes.Category), "Unknown tax category")
                };

                line.Gross = line.Quantity * line.UnitPrice * line.Taxes.Rate;
                line.Net = line.Quantity * line.UnitPrice;

                line.Gross = line.Gross.RoundKeepSign(2);
                line.Net = line.Net.RoundKeepSign(2);

                entry.Base += line.Net;
                entry.Value += line.Gross - line.Net;

                totals.Net += line.Net;
                totals.Gross += line.Gross;

                if (line.Settlements != null)
                {
                    var totalSettlement = line.Settlements.Sum(x => x.Amount);
                    
                    totals.Gross += totalSettlement;
                    totals.Net += totalSettlement / line.Taxes.Rate;
                }
            }

            return totals;
        }
    }
}