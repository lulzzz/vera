using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vera.Models;

namespace Vera.Invoices.Validation
{
    public class TotalPaidValidator : IInvoiceValidator
    {
        public IEnumerable<ValidationResult> Validate(Invoice invoice)
        {
            var calculator = new InvoiceTotalsCalculator();
            var totals = calculator.Calculate(invoice);

            var gross = Math.Round(totals.Gross, 2);
            var totalPayments = Math.Round(invoice.Payments.Sum(p => p.Amount), 2);

            if (gross != totalPayments)
            {
                yield return new ValidationResult(
                    $"gross is {gross:F} but there is only {totalPayments:F} paid",
                    new[] { nameof(invoice.Totals) }
                );
            }
        }
    }
}
