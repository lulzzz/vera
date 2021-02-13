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

            Assert.Contains(results, x => x.MemberNames.Contains("Customer"));
        }

        [Fact]
        public void Should_require_specific_fields_on_billing_address()
        {
            var tests = new[]
            {
                new
                {
                    Expected = new[] { "Street", "City", "PostalCode", "Country" },
                    Address = new Address
                    {
                    },
                },
                new
                {
                    Expected = new[] { "City", "PostalCode", "Country" },
                    Address = new Address
                    {
                        Street = "Some street"
                    },
                },
                new
                {
                    Expected = new[] { "PostalCode", "Country" },
                    Address = new Address
                    {
                        Street = "Some street",
                        City = "Some city"
                    },
                },
                new
                {
                    Expected = new[] { "Country" },
                    Address = new Address
                    {
                        Street = "Some street",
                        City = "Some city",
                        PostalCode = "1234-100"
                    },
                },
            };
            
            var invoice = new Invoice
            {
                Customer = new Customer(),
                Totals = new Totals
                {
                    Gross = InvoiceTypeHelper.FaturaInvoiceLimit + 1m
                }
            };

            foreach (var test in tests)
            {
                invoice.Customer.BillingAddress = test.Address;

                var results = RunValidator(invoice);
                var allMemberNames = results.SelectMany(x => x.MemberNames).ToList();

                foreach (var expected in test.Expected)
                {
                    Assert.Contains($"BillingAddress.{expected}", allMemberNames);
                }
            }
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