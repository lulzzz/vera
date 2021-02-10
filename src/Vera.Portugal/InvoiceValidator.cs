using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vera.Invoices;
using Vera.Models;

namespace Vera.Portugal
{
    public class InvoiceValidator : IInvoiceValidator
    {
        public ICollection<ValidationResult> Validate(Invoice invoice)
        {
            var results = new List<ValidationResult>();
            
            ValidateFaturaInvoiceLimit(invoice, results);
            ValidateMixedQuantities(invoice, results);
            ValidateTaxExemption(invoice, results);

            return results;
        }

        private static void ValidateFaturaInvoiceLimit(Invoice invoice, ICollection<ValidationResult> results)
        {
            if (invoice.Totals.Gross <= InvoiceTypeHelper.FaturaInvoiceLimit) return;
            
            if (invoice.Customer == null)
            {
                results.Add(new ValidationResult(
                    $"gross > {InvoiceTypeHelper.FaturaInvoiceLimit}, customer is required",
                    new[] {nameof(invoice.Customer)}
                ));
            }

            // TODO(kevin): move billing to the root level of the invoice
            if (invoice.Customer?.BillingAddress == null)
            {
                results.Add(new ValidationResult(
                    $"gross > {InvoiceTypeHelper.FaturaInvoiceLimit}, billing address is required",
                    new[] {nameof(invoice.Customer.BillingAddress)}
                ));
            }
        }

        private static void ValidateMixedQuantities(Invoice invoice, ICollection<ValidationResult> results)
        {
            var hasPositiveLines = false;
            var hasNegativeLines = false;
            foreach (var line in invoice.Lines)
            {
                hasPositiveLines |= line.Quantity > 0;
                hasNegativeLines |= line.Quantity < 0;

                if (!hasPositiveLines || !hasNegativeLines) continue;
                
                results.Add(new ValidationResult(
                    "not allowed to have mixed quantities",
                    new[] {nameof(invoice.Lines)}
                ));

                break;
            }
        }
        
        private static void ValidateTaxExemption(Invoice invoice, ICollection<ValidationResult> results)
        {
            foreach (var line in invoice.Lines)
            {
                var taxes = line.Taxes;
                if (taxes.Category == TaxesCategory.Exempt && string.IsNullOrWhiteSpace(taxes.ExemptionReason))
                {
                    results.Add(new ValidationResult(
                        $"line '{line.Description}' is exempted from tax, but the reason is missing",
                        new[] { nameof(line.Taxes.ExemptionReason) }
                    ));
                }
            }
        }
    }
}