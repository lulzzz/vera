using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vera.Models;

namespace Vera.Invoices.Validation
{
    public class CurrencyValidator : IInvoiceValidator
    {
        public IEnumerable<ValidationResult> Validate(Invoice invoice)
        {
            if (!Currencies.CodeToCurrencyMapper.ContainsKey(invoice.CurrencyCode))
            {
                yield return new ValidationResult(
                    $"The currency {invoice.CurrencyCode} is not valid"
                );
            }
        }
    }
}
