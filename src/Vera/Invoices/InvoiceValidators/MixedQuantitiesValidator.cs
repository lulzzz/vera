using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vera.Models;

namespace Vera.Invoices.InvoiceValidators
{
    public class MixedQuantitiesValidator : IInvoiceValidator
    {
        public IEnumerable<ValidationResult> Validate(Invoice invoice)
        {
            var hasPositiveLines = false;
            var hasNegativeLines = false;

            foreach (var line in invoice.Lines)
            {
                hasPositiveLines |= line.Quantity > 0;
                hasNegativeLines |= line.Quantity < 0;

                if (!hasPositiveLines || !hasNegativeLines) continue;

                yield return new ValidationResult(
                    "not allowed to have mixed quantities",
                    new[] { nameof(invoice.Lines) });

                break;
            }
        }
    }
}
