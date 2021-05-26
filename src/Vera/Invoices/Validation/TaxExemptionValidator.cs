using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vera.Models;

namespace Vera.Invoices.Validation
{
    public class TaxExemptionValidator : IInvoiceValidator
    {
        public IEnumerable<ValidationResult> Validate(Invoice invoice)
        {
            return
                from line in invoice.Lines
                let taxes = line.Taxes
                where taxes.Category == TaxesCategory.Exempt && string.IsNullOrWhiteSpace(taxes.ExemptionReason)
                select new ValidationResult(
                    $"line '{line.Description}' is exempted from tax, but the reason is missing",
                    new[] { nameof(line.Taxes.ExemptionReason) });
        }
    }
}
