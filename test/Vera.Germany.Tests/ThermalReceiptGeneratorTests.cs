using Bogus;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Vera.Documents.Visitors;
using Vera.Invoices;
using Vera.Models;
using Vera.Tests.Shared;
using Vera.Thermal;
using Xunit;
using Xunit.Abstractions;

namespace Vera.Germany.Tests
{
    public class ThermalReceiptGeneratorTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public ThermalReceiptGeneratorTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Should_generate_receipt()
        {
            var invoice = CreateTestInvoice();

            var context = new ThermalReceiptContext
            {
                Account = null,
                Invoice = invoice,
                Original = true,
                SoftwareVersion = "EVA Unified Commerce version 2.0"
            };

            var result = GenerateReceipt(context);

            Assert.Contains(invoice.Supplier.Name, result);
            Assert.Contains(invoice.Supplier.Address.Street, result);
            Assert.Contains(invoice.Supplier.Address.Number, result);
            Assert.Contains(invoice.Supplier.Address.PostalCode, result);
            Assert.Contains(invoice.Supplier.Address.City, result);
            Assert.Contains(invoice.Supplier.Address.Country, result);

            Assert.Contains("KAUFBELEG", result);

            Assert.Contains("Original", result);
            Assert.DoesNotContain("Dublicate", result);
            Assert.DoesNotContain("Referenzdokument", result);

            Assert.Contains("Endverbraucher", result);

            Assert.Contains(invoice.Lines.First().Product.Description, result);

            _testOutputHelper.WriteLine(result);
        }

        [Fact]
        public void Should_generate_receipt_duplicate()
        {
            var invoice = CreateTestInvoice();

            var context = new ThermalReceiptContext
            {
                Account = null,
                Invoice = invoice,
                Original = false,
                SoftwareVersion = "EVA Unified Commerce version 2.0",
            };

            var result = GenerateReceipt(context);

            Assert.Contains(invoice.Supplier.Name, result);
            Assert.Contains(invoice.Supplier.Address.Street, result);
            Assert.Contains(invoice.Supplier.Address.Number, result);
            Assert.Contains(invoice.Supplier.Address.PostalCode, result);
            Assert.Contains(invoice.Supplier.Address.City, result);
            Assert.Contains(invoice.Supplier.Address.Country, result);

            Assert.Contains("KAUFBELEG", result);

            Assert.Contains("Dublicate", result);
            Assert.DoesNotContain("Original", result);
            Assert.DoesNotContain("Referenzdokument", result);

            Assert.Contains("Endverbraucher", result);

            Assert.Contains(invoice.Lines.First().Product.Description, result);

            _testOutputHelper.WriteLine(result);
        }

        [Fact]
        public void Should_generate_return_receipt()
        {
            var returnInvoice = CreateTestInvoice();
            var invoice = CreateTestInvoice();
            invoice.ReturnedInvoiceNumber = returnInvoice.Number;

            var context = new ThermalReceiptContext
            {
                Account = null,
                Invoice = invoice,
                Original = true,
                SoftwareVersion = "EVA Unified Commerce version 2.0",
            };

            var result = GenerateReceipt(context);

            Assert.Contains(invoice.Supplier.Name, result);
            Assert.Contains(invoice.Supplier.Address.Street, result);
            Assert.Contains(invoice.Supplier.Address.Number, result);
            Assert.Contains(invoice.Supplier.Address.PostalCode, result);
            Assert.Contains(invoice.Supplier.Address.City, result);
            Assert.Contains(invoice.Supplier.Address.Country, result);

            Assert.Contains("RÜCKSCHEIN", result);

            Assert.Contains("Original", result);
            Assert.DoesNotContain("Dublicate", result);
            Assert.Contains("Referenzdokument", result);

            Assert.Contains("Endverbraucher", result);

            Assert.Contains(invoice.Lines.First().Product.Description, result);

            _testOutputHelper.WriteLine(result);
        }

        [Fact]
        public void Should_generate_return_receipt_duplicate()
        {
            var returnInvoice = CreateTestInvoice();
            var invoice = CreateTestInvoice();
            invoice.ReturnedInvoiceNumber = returnInvoice.Number;

            var context = new ThermalReceiptContext
            {
                Account = null,
                Invoice = invoice,
                Original = false,
                SoftwareVersion = "EVA Unified Commerce version 2.0",
            };

            var result = GenerateReceipt(context);

            Assert.Contains(invoice.Supplier.Name, result);
            Assert.Contains(invoice.Supplier.Address.Street, result);
            Assert.Contains(invoice.Supplier.Address.Number, result);
            Assert.Contains(invoice.Supplier.Address.PostalCode, result);
            Assert.Contains(invoice.Supplier.Address.City, result);
            Assert.Contains(invoice.Supplier.Address.Country, result);

            Assert.Contains("RÜCKSCHEIN", result);

            Assert.DoesNotContain("Original", result);
            Assert.Contains("Dublicate", result);
            Assert.Contains("Referenzdokument", result);

            Assert.Contains("Endverbraucher", result);

            Assert.Contains(invoice.Lines.First().Product.Description, result);

            _testOutputHelper.WriteLine(result);
        }

        [Fact]
        public void Should_generate_receipt_with_customer()
        {
            var invoice = CreateTestInvoice(true);

            var context = new ThermalReceiptContext
            {
                Account = null,
                Invoice = invoice,
                Original = true,
                SoftwareVersion = "EVA Unified Commerce version 2.0"
            };

            var result = GenerateReceipt(context);

            Assert.Contains(invoice.Supplier.Name, result);
            Assert.Contains(invoice.Supplier.Address.Street, result);
            Assert.Contains(invoice.Supplier.Address.Number, result);
            Assert.Contains(invoice.Supplier.Address.PostalCode, result);
            Assert.Contains(invoice.Supplier.Address.City, result);
            Assert.Contains(invoice.Supplier.Address.Country, result);

            Assert.Contains("KAUFBELEG", result);

            Assert.Contains("Original", result);
            Assert.DoesNotContain("Dublicate", result);
            Assert.DoesNotContain("Referenzdokument", result);

            Assert.Contains(invoice.Customer.SystemId, result);
            Assert.Contains(invoice.Customer.FirstName, result);
            Assert.Contains(invoice.Customer.LastName, result);
            Assert.Contains(invoice.Customer.BillingAddress.Street, result);
            Assert.Contains(invoice.Customer.BillingAddress.Number, result);
            Assert.Contains(invoice.Customer.BillingAddress.PostalCode, result);
            Assert.Contains(invoice.Customer.BillingAddress.City, result);
            Assert.Contains(invoice.Customer.BillingAddress.Country, result);

            Assert.Contains(invoice.Customer.RegistrationNumber, result);

            Assert.Contains(invoice.Lines.First().Product.Description, result);

            _testOutputHelper.WriteLine(result);
        }

        [Fact]
        public void Should_generate_receipt_with_customer_duplicate()
        {
            var invoice = CreateTestInvoice(true);

            var context = new ThermalReceiptContext
            {
                Account = null,
                Invoice = invoice,
                Original = false,
                SoftwareVersion = "EVA Unified Commerce version 2.0"
            };

            var result = GenerateReceipt(context);

            Assert.Contains(invoice.Supplier.Name, result);
            Assert.Contains(invoice.Supplier.Address.Street, result);
            Assert.Contains(invoice.Supplier.Address.Number, result);
            Assert.Contains(invoice.Supplier.Address.PostalCode, result);
            Assert.Contains(invoice.Supplier.Address.City, result);
            Assert.Contains(invoice.Supplier.Address.Country, result);

            Assert.Contains("KAUFBELEG", result);

            Assert.DoesNotContain("Original", result);
            Assert.Contains("Dublicate", result);
            Assert.DoesNotContain("Referenzdokument", result);

            Assert.Contains(invoice.Customer.SystemId, result);
            Assert.Contains(invoice.Customer.FirstName, result);
            Assert.Contains(invoice.Customer.LastName, result);
            Assert.Contains(invoice.Customer.BillingAddress.Street, result);
            Assert.Contains(invoice.Customer.BillingAddress.Number, result);
            Assert.Contains(invoice.Customer.BillingAddress.PostalCode, result);
            Assert.Contains(invoice.Customer.BillingAddress.City, result);
            Assert.Contains(invoice.Customer.BillingAddress.Country, result);

            Assert.Contains(invoice.Customer.RegistrationNumber, result);

            Assert.Contains(invoice.Lines.First().Product.Description, result);

            _testOutputHelper.WriteLine(result);
        }

        [Theory]
        [InlineData(null)]
        public void Should_generate_receipt_with_customer_null_registration_number(string registrationNumber)
        {
            var invoice = CreateTestInvoice(true);
            invoice.Customer.RegistrationNumber = registrationNumber;

            var context = new ThermalReceiptContext
            {
                Account = null,
                Invoice = invoice,
                Original = true,
                SoftwareVersion = "EVA Unified Commerce version 2.0"
            };

            var result = GenerateReceipt(context);

            Assert.Contains(invoice.Supplier.Name, result);
            Assert.Contains(invoice.Supplier.Address.Street, result);
            Assert.Contains(invoice.Supplier.Address.Number, result);
            Assert.Contains(invoice.Supplier.Address.PostalCode, result);
            Assert.Contains(invoice.Supplier.Address.City, result);
            Assert.Contains(invoice.Supplier.Address.Country, result);

            Assert.Contains("KAUFBELEG", result);

            Assert.Contains("Original", result);
            Assert.DoesNotContain("Dublicate", result);
            Assert.DoesNotContain("Referenzdokument", result);

            Assert.Contains(invoice.Customer.SystemId, result);
            Assert.Contains(invoice.Customer.FirstName, result);
            Assert.Contains(invoice.Customer.LastName, result);
            Assert.Contains(invoice.Customer.BillingAddress.Street, result);
            Assert.Contains(invoice.Customer.BillingAddress.Number, result);
            Assert.Contains(invoice.Customer.BillingAddress.PostalCode, result);
            Assert.Contains(invoice.Customer.BillingAddress.City, result);
            Assert.Contains(invoice.Customer.BillingAddress.Country, result);

            Assert.Contains("Endverbraucher", result);

            Assert.Contains(invoice.Lines.First().Product.Description, result);

            _testOutputHelper.WriteLine(result);
        }

        [Theory]
        [InlineData(null)]
        public void Should_generate_receipt_with_customer_null_registration_number_duplicate(string registrationNumber)
        {
            var invoice = CreateTestInvoice(true);
            invoice.Customer.RegistrationNumber = registrationNumber;

            var context = new ThermalReceiptContext
            {
                Account = null,
                Invoice = invoice,
                Original = false,
                SoftwareVersion = "EVA Unified Commerce version 2.0"
            };

            var result = GenerateReceipt(context);

            Assert.Contains(invoice.Supplier.Name, result);
            Assert.Contains(invoice.Supplier.Address.Street, result);
            Assert.Contains(invoice.Supplier.Address.Number, result);
            Assert.Contains(invoice.Supplier.Address.PostalCode, result);
            Assert.Contains(invoice.Supplier.Address.City, result);
            Assert.Contains(invoice.Supplier.Address.Country, result);

            Assert.Contains("KAUFBELEG", result);

            Assert.DoesNotContain("Original", result);
            Assert.Contains("Dublicate", result);
            Assert.DoesNotContain("Referenzdokument", result);

            Assert.Contains(invoice.Customer.SystemId, result);
            Assert.Contains(invoice.Customer.FirstName, result);
            Assert.Contains(invoice.Customer.LastName, result);
            Assert.Contains(invoice.Customer.BillingAddress.Street, result);
            Assert.Contains(invoice.Customer.BillingAddress.Number, result);
            Assert.Contains(invoice.Customer.BillingAddress.PostalCode, result);
            Assert.Contains(invoice.Customer.BillingAddress.City, result);
            Assert.Contains(invoice.Customer.BillingAddress.Country, result);

            Assert.Contains("Endverbraucher", result);

            Assert.Contains(invoice.Lines.First().Product.Description, result);

            _testOutputHelper.WriteLine(result);
        }

        [Fact]
        public void Should_generate_receipt_with_product_settlement()
        {
            var invoice = CreateTestInvoice(false, true);

            var context = new ThermalReceiptContext
            {
                Account = null,
                Invoice = invoice,
                Original = true,
                SoftwareVersion = "EVA Unified Commerce version 2.0"
            };

            var productWithSettlement = invoice.Lines.Where(p => p.Settlements.Count > 0).First();

            var result = GenerateReceipt(context);

            Assert.Contains(invoice.Supplier.Name, result);
            Assert.Contains(invoice.Supplier.Address.Street, result);
            Assert.Contains(invoice.Supplier.Address.Number, result);
            Assert.Contains(invoice.Supplier.Address.PostalCode, result);
            Assert.Contains(invoice.Supplier.Address.City, result);
            Assert.Contains(invoice.Supplier.Address.Country, result);

            Assert.Contains("KAUFBELEG", result);

            Assert.Contains("Original", result);
            Assert.DoesNotContain("Dublicate", result);
            Assert.DoesNotContain("Referenzdokument", result);

            Assert.Contains("Endverbraucher", result);

            Assert.Contains(invoice.Lines.First().Product.Description, result);

            Assert.Contains(productWithSettlement.Settlements.First().Amount.ToString(), result);

            _testOutputHelper.WriteLine(result);
        }

        [Fact]
        public void Should_generate_receipt_with_product_settlement_duplicate()
        {
            var invoice = CreateTestInvoice(false, true);

            var context = new ThermalReceiptContext
            {
                Account = null,
                Invoice = invoice,
                Original = false,
                SoftwareVersion = "EVA Unified Commerce version 2.0"
            };

            var productWithSettlement = invoice.Lines.Where(p => p.Settlements.Count > 0).First();

            var result = GenerateReceipt(context);

            Assert.Contains(invoice.Supplier.Name, result);
            Assert.Contains(invoice.Supplier.Address.Street, result);
            Assert.Contains(invoice.Supplier.Address.Number, result);
            Assert.Contains(invoice.Supplier.Address.PostalCode, result);
            Assert.Contains(invoice.Supplier.Address.City, result);
            Assert.Contains(invoice.Supplier.Address.Country, result);

            Assert.Contains("KAUFBELEG", result);

            Assert.DoesNotContain("Original", result);
            Assert.Contains("Dublicate", result);
            Assert.DoesNotContain("Referenzdokument", result);

            Assert.Contains("Endverbraucher", result);

            Assert.Contains(invoice.Lines.First().Product.Description, result);

            Assert.Contains(productWithSettlement.Settlements.First().Amount.ToString(), result);

            _testOutputHelper.WriteLine(result);
        }

        private static string GenerateReceipt(ThermalReceiptContext context)
        {
            var generator = new ThermalReceiptGenerator();
            var node = generator.Generate(context);

            var sb = new StringBuilder();

            var visitor = new TextThermalVisitor(new StringWriter(sb));
            node.Accept(visitor);

            return sb.ToString();
        }

        private static Invoice CreateTestInvoice(bool withCustomer = false, bool withDiscount = false)
        {
            var product = ProductFactory.CreateCocaCola();

            var builder = new InvoiceBuilder();

            const decimal amountInTax = 1.99m;

            var invoice = builder
                .Reset()
                .WithRegister("1.1")
                .WithEmployee()
                .WithSupplier("123")
                .WithPayment(amountInTax, PaymentCategory.Cash)
                .WithProductLine(1, amountInTax, 1.21m, TaxesCategory.High, product)
                .Result;

            if (withCustomer)
            {
                builder.WithCustomer();
            }

            if (withDiscount)
            {
                builder.WithProductLineSettlement(product, TaxesCategory.Intermediate, amountInTax, 1.21m, 1.19m);
            }

            invoice.AccountId = Guid.NewGuid();
            invoice.Signature = new Signature { Output = new byte[32] };
            invoice.Number = new Faker().Random.Number(1, 10_000).ToString();
            invoice.Totals = new InvoiceTotalsCalculator().Calculate(invoice);

            return invoice;
        }
    }
}
