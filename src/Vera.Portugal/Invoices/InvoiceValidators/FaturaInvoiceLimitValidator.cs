using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Vera.Invoices;
using Vera.Models;

namespace Vera.Portugal.Invoices.InvoiceValidators
{
    public class FaturaInvoiceLimitValidator : IInvoiceValidator
    {
        private readonly Regex _postalCodeFormat = new(@"\d{4}\-\d{3}", RegexOptions.Compiled);

        public IEnumerable<ValidationResult> Validate(Invoice invoice)
        {
            if (invoice.Totals.Gross <= InvoiceTypeHelper.FaturaInvoiceLimit) yield break;

            if (invoice.Customer == null)
            {
                yield return new ValidationResult(
                    $"gross > {InvoiceTypeHelper.FaturaInvoiceLimit}, customer is required",
                    new[] { "Customer" }
                );

                yield break;
            }

            // TODO(kevin): move billing to the root level of the invoice
            if (invoice.Customer?.BillingAddress == null)
            {
                yield return new ValidationResult(
                    $"gross > {InvoiceTypeHelper.FaturaInvoiceLimit}, billing address is required",
                    new[] { "BillingAddress" }
                );

                yield break;
            }

            var billingAddress = invoice.Customer.BillingAddress;

            if (string.IsNullOrEmpty(billingAddress.Street))
            {
                yield return new ValidationResult(
                    "street is required",
                    new[] { "BillingAddress.Street" }
                );
            }

            if (string.IsNullOrEmpty(billingAddress.City))
            {
                yield return new ValidationResult(
                    "city is required",
                    new[] { "BillingAddress.City" }
                );
            }

            if (string.IsNullOrEmpty(billingAddress.PostalCode) ||
                !_postalCodeFormat.IsMatch(billingAddress.PostalCode))
            {
                yield return new ValidationResult(
                    "invalid postal code format",
                    new[] { "BillingAddress.PostalCode" }
                );
            }

            if (string.IsNullOrEmpty(billingAddress.Country))
            {
                yield return new ValidationResult(
                    "country is required",
                    new[] { "BillingAddress.Country" }
                );
            }
            else if (billingAddress.Country.Length != 2)
            {
                yield return new ValidationResult(
                    "country is invalid format, expected ISO-3166 alpha-2",
                    new[] { "BillingAddress.Country" }
                );
            }
        }
    }
}
