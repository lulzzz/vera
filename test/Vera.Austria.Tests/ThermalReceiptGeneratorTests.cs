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

namespace Vera.Austria.Tests
{
    public class ThermalReceiptGeneratorTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly InvoiceBuilder _invoiceGenerator;

        public ThermalReceiptGeneratorTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _invoiceGenerator = new InvoiceBuilder(new Faker());
        }

        [Fact]
        public void Should_generate_receipt()
        {
            const string certificateNumber = "1234Abc";
            var invoice = CreateTestInvoice();

            var context = new ThermalReceiptContext
            {
                Account = null,
                Invoice = invoice,
                Original = true,
                SoftwareVersion = "EVA Unified Commerce version 2.0",
            };

            var generator = new ThermalReceiptGenerator(certificateNumber);
            var node = generator.Generate(context);

            var sb = new StringBuilder();

            var visitor = new StringThermalVisitor(new StringWriter(sb));
            node.Accept(visitor);

            var result = sb.ToString();

            Assert.Contains("KAUFBELEG", result);

            Assert.Contains(invoice.Supplier.Name, result);
            Assert.Contains(invoice.Supplier.Address.Street, result);
            Assert.Contains(invoice.Supplier.Address.Number, result);
            Assert.Contains(invoice.Supplier.Address.PostalCode, result);
            Assert.Contains(invoice.Supplier.Address.City, result);

            Assert.Contains("Original", result);
            Assert.Contains(invoice.Number, result);
            Assert.DoesNotContain("Dublicate", result);
            Assert.DoesNotContain("Referenzdokument", result);

            Assert.Contains("Zahlungsmethoden", result);

            Assert.Contains("Barzahlung", result);


            Assert.Contains(invoice.Lines.First().Product.Description, result);

            _testOutputHelper.WriteLine(result);
        }

        [Fact]
        public void Should_generate_receipt_duplicate()
        {
            const string certificateNumber = "1234Abc";
            var invoice = CreateTestInvoice();

            var context = new ThermalReceiptContext
            {
                Account = null,
                Invoice = invoice,
                Original = false,
                SoftwareVersion = "EVA Unified Commerce version 2.0",
            };

            var generator = new ThermalReceiptGenerator(certificateNumber);
            var node = generator.Generate(context);

            var sb = new StringBuilder();

            var visitor = new StringThermalVisitor(new StringWriter(sb));
            node.Accept(visitor);

            var result = sb.ToString();

            Assert.Contains("Dublicate", result);

            Assert.DoesNotContain("Original", result);

            _testOutputHelper.WriteLine(result);
        }


        [Fact]
        public void Should_generate_return_receipt()
        {
            const string certificateNumber = "1234Abc";
            var returnInvoice = CreateTestInvoice();
            var invoice = CreateTestReturnInvoice(returnInvoice);

            var context = new ThermalReceiptContext
            {
                Account = null,
                Invoice = invoice,
                Original = true,
                SoftwareVersion = "EVA Unified Commerce version 2.0",
            };

            var generator = new ThermalReceiptGenerator(certificateNumber);
            var node = generator.Generate(context);

            var sb = new StringBuilder();

            var visitor = new StringThermalVisitor(new StringWriter(sb));
            node.Accept(visitor);

            var result = sb.ToString();

            Assert.Contains("Referenzdokument", result);

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

            const decimal unitPrice = 1.99m;
            const decimal rate = 1.21m;
            const int quantity = 1;

            var invoice = _invoiceGenerator
                .Reset()
                .WithTerminal("1.1")
                .WithEmployee()
                .WithSupplier()
                .WithPayment(unitPrice, PaymentCategory.Cash)
                .WithProductLine(product, TaxesCategory.High, unitPrice, quantity, rate)
                .Build();

            invoice.AccountId = Guid.NewGuid();
            invoice.Signature = new Signature { Output = new byte[32] };
            invoice.Number = Guid.NewGuid().ToString();
            invoice.Totals = new InvoiceTotalsCalculator().Calculate(invoice);

            return invoice;
        }

        private Invoice CreateTestReturnInvoice(Invoice returnInvoice)
        {
            var invoice = CreateTestInvoice();
            var line = invoice.Lines.First();
            line.CreditReference = new CreditReference
            {
                Number = returnInvoice.Number,
                Reason = "return invoice"
            };

            return invoice;
        }
    }
}
