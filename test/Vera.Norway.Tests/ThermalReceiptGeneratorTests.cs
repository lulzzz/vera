using System;
using System.IO;
using System.Text;
using Bogus;
using Vera.Documents.Visitors;
using Vera.Invoices;
using Vera.Models;
using Vera.Tests.Shared;
using Vera.Thermal;
using Xunit;
using Xunit.Abstractions;

namespace Vera.Norway.Tests
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
                SoftwareVersion = "EVA Unified Commerce version 2.0",
            };

            var generator = new ThermalReceiptGenerator();
            var node = generator.Generate(context);

            var sb = new StringBuilder();

            var visitor = new StringThermalVisitor(new StringWriter(sb));
            node.Accept(visitor);

            var result = sb.ToString();
            
            Assert.Contains("Salgskvittering", result);
            
            Assert.Contains(invoice.Supplier.Name, result);
            Assert.Contains(invoice.Supplier.Address.Street, result);
            Assert.Contains(invoice.Supplier.Address.Number, result);
            Assert.Contains(invoice.Supplier.Address.PostalCode, result);
            Assert.Contains(invoice.Supplier.Address.City, result);

            Assert.Contains("Foretaksregisteret", result);
            Assert.Contains("Kvitteringnummer", result);
            Assert.Contains("OPPRINNELIG", result);
            Assert.Contains("Sluttforbruker", result);

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

            var generator = new ThermalReceiptGenerator();
            var node = generator.Generate(context);

            var sb = new StringBuilder();

            var visitor = new StringThermalVisitor(new StringWriter(sb));
            node.Accept(visitor);

            var result = sb.ToString();

            Assert.Contains("KOPI", result);

            Assert.DoesNotContain("DOBBELTTRYKK", result);
            Assert.DoesNotContain("OPPRINNELIG", result);

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

            var generator = new ThermalReceiptGenerator();
            var node = generator.Generate(context);

            var sb = new StringBuilder();

            var visitor = new StringThermalVisitor(new StringWriter(sb));
            node.Accept(visitor);

            var result = sb.ToString();

            Assert.Contains("Returkvittering", result);

            Assert.Contains("Kvitteringnummer", result);
            Assert.Contains(returnInvoice.Number, result);

            Assert.DoesNotContain("DOBBELTTRYKK", result);

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

            var generator = new ThermalReceiptGenerator();
            var node = generator.Generate(context);

            var sb = new StringBuilder();

            var visitor = new StringThermalVisitor(new StringWriter(sb));
            node.Accept(visitor);

            var result = sb.ToString();

            Assert.Contains("KOPI", result);

            Assert.Contains("DOBBELTTRYKK", result);

            _testOutputHelper.WriteLine(result);
        }

        private Invoice CreateTestInvoice()
        {
            var product = new Product
            {
                Code = "COCA",
                Description = "Coca cola",
                Type = ProductType.Goods
            };
            
            var builder = new InvoiceBuilder();

            const decimal amountInTax = 1.99m;

            var invoice = builder
                .Reset()
                .WithTerminal("1.1")
                .WithEmployee()
                .WithSupplier("123")
                .WithPayment(amountInTax, PaymentCategory.Cash)
                .WithProductLine(1, amountInTax, 1.21m, TaxesCategory.High, product)
                .Result;

            invoice.AccountId = Guid.NewGuid();
            invoice.Signature = new Signature { Output = new byte[32] };
            invoice.Number = Guid.NewGuid().ToString();
            invoice.Totals = new InvoiceTotalsCalculator().Calculate(invoice);

            return invoice;
        }
    }
}