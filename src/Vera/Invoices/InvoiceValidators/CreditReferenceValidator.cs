using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vera.Models;

namespace Vera.Invoices.InvoiceValidators
{
    public class CreditReferenceValidator : IInvoiceValidator
    {
        public IEnumerable<ValidationResult> Validate(Invoice invoice)
        {
            return
                from line in invoice.Lines
                where line.Quantity <= 0
                where string.IsNullOrEmpty(line.CreditReference?.Number)
                select new ValidationResult(
                    $"line '{line.Description}' is credited, but the credit reference is missing",
                    new[] { nameof(line.CreditReference) });
        }
    }
}
