using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vera.Models;
using Xunit;

namespace Vera.Portugal.Tests
{
    public class InvoiceValidatorTests
    {
        [Fact]
        public void Should_require_customer_above_fatura_limit()
        {
            var invoice = new Invoice
            {
                Customer = null,
                Totals = new Totals
                {
                    Gross = InvoiceTypeHelper.FaturaInvoiceLimit + 1m
                }
            };

            var results = RunValidator(invoice);

            Assert.NotEmpty(results);
            Assert.Contains(results, x => x.MemberNames.Contains("Customer"));
        }

        [Fact]
        public void Should_not_allow_mixed_quantities()
        {
            var invoice = new Invoice
            {
                Lines = new List<InvoiceLine>
                {
                    new()
                    {
                        Quantity = 1,
                        Taxes = new Taxes()
                    },
                    new()
                    {
                        Quantity = -1,
                        Taxes = new Taxes()
                    }
                }
            };

            var results = RunValidator(invoice);

            Assert.Contains(results, x => x.MemberNames.Contains("Lines"));
        }

        [Fact]
        public void Should_require_tax_exemption_reason()
        {
            var tests = new[]
            {
                new Taxes()
                {
                    Category = TaxesCategory.Exempt,
                    ExemptionCode = null,
                    ExemptionReason = null
                },
                new Taxes()
                {
                    Category = TaxesCategory.Exempt,
                    ExemptionCode = null,
                    ExemptionReason = string.Empty
                },
                new Taxes()
                {
                    Category = TaxesCategory.Exempt,
                    ExemptionCode = null,
                    ExemptionReason = " "
                }
            };

            foreach (var test in tests)
            {
                var invoice = new Invoice
                {
                    Lines = new List<InvoiceLine>
                    {
                        new()
                        {
                            Taxes = test
                        }
                    }
                };

                var results = RunValidator(invoice);

                Assert.Contains(results, x => x.MemberNames.Contains("ExemptionReason"));
            }
        }

        private static ICollection<ValidationResult> RunValidator(Invoice invoice)
        {
            var validator = new InvoiceValidator();
            var results = validator.Validate(invoice);
            return results;
        }
    }
}