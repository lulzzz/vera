using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Vera.Invoices;
using Vera.Models;

namespace Vera.Portugal
{
    public class InvoiceValidator : IInvoiceValidator
    {
        private readonly Regex _postalCodeFormat = new(@"\d{4}\-\d{3}", RegexOptions.Compiled);

        public ICollection<ValidationResult> Validate(Invoice invoice)
        {
            var results = new List<ValidationResult>();

            results.AddRange(ValidateFaturaInvoiceLimit(invoice));

            ValidateMixedQuantities(invoice, results);
            ValidateTaxExemption(invoice, results);

            // TODO(kevin): validate that a return line has a reference to the original line

            return results;
        }

        private IEnumerable<ValidationResult> ValidateFaturaInvoiceLimit(Invoice invoice)
        {
            if (invoice.Totals.Gross <= InvoiceTypeHelper.FaturaInvoiceLimit) yield break;

            if (invoice.Customer == null)
            {
                yield return new ValidationResult(
                    $"gross > {InvoiceTypeHelper.FaturaInvoiceLimit}, customer is required",
                    new[] {"Customer"}
                );

                yield break;
            }

            // TODO(kevin): move billing to the root level of the invoice
            if (invoice.Customer?.BillingAddress == null)
            {
                yield return new ValidationResult(
                    $"gross > {InvoiceTypeHelper.FaturaInvoiceLimit}, billing address is required",
                    new[] {"BillingAddress"}
                );

                yield break;
            }

            var billingAddress = invoice.Customer.BillingAddress;

            if (string.IsNullOrEmpty(billingAddress.Street))
            {
                yield return new ValidationResult(
                    "street is required",
                    new[] {"BillingAddress.Street"}
                );
            }

            if (string.IsNullOrEmpty(billingAddress.City))
            {
                yield return new ValidationResult(
                    "city is required",
                    new[] {"BillingAddress.City"}
                );
            }

            if (string.IsNullOrEmpty(billingAddress.PostalCode) ||
                !_postalCodeFormat.IsMatch(billingAddress.PostalCode))
            {
                yield return new ValidationResult(
                    "invalid postal code format",
                    new[] {"BillingAddress.PostalCode"}
                );
            }

            if (string.IsNullOrEmpty(billingAddress.Country))
            {
                yield return new ValidationResult(
                    "country is required",
                    new[]{"BillingAddress.Country"}
                );
            }
            else if (billingAddress.Country.Length != 2)
            {
                yield return new ValidationResult(
                    "country is invalid format, expected ISO-3166 alpha-2",
                    new[] {"BillingAddress.Country"}
                );
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
                        new[] {nameof(line.Taxes.ExemptionReason)}
                    ));
                }
            }
        }
    }
}