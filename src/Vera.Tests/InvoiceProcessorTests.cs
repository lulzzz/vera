using System;
using System.Threading.Tasks;
using Bogus;
using Moq;
using Vera.Concurrency;
using Vera.Invoices;
using Vera.Models;
using Vera.Signing;
using Vera.Stores;
using Vera.Tests.Shared;
using Xunit;

namespace Vera.Tests
{
    public class InvoiceProcessorTests
    {
        [Fact]
        public async Task Should_process_invoice()
        {
            const string expectedBucket = "my-bucket";
            const string expectedNumber = "my-invoice-no";

            var store = new Mock<IInvoiceStore>();
            var factory = new Mock<IComponentFactory>();

            var bucketGenerator = new Mock<IInvoiceBucketGenerator>();
            var invoiceNumberGenerator = new Mock<IInvoiceNumberGenerator>();
            var signer = new Mock<IPackageSigner>();

            factory.Setup(f => f.CreateInvoiceBucketGenerator())
                .Returns(bucketGenerator.Object);

            factory.Setup(f => f.CreateInvoiceNumberGenerator())
                .Returns(invoiceNumberGenerator.Object);

            factory.Setup(f => f.CreatePackageSigner())
                .Returns(signer.Object);

            bucketGenerator.Setup(x => x.Generate(It.IsAny<Invoice>()))
                .Returns(expectedBucket);

            invoiceNumberGenerator.Setup(x => x.Generate(It.IsAny<Invoice>()))
                .ReturnsAsync(expectedNumber);

            signer.Setup(x => x.Sign(It.IsAny<Package>()))
                .ReturnsAsync(new Signature());

            var processor = new InvoiceProcessor(
                store.Object,
                new NullLocker(),
                factory.Object
            );

            var invoiceGenerator = new InvoiceGenerator(new Faker());
            var invoice = invoiceGenerator.CreateWithCustomerAndSingleProduct(Guid.Empty.ToString());

            // Should be set after processing the invoice
            Assert.Null(invoice.Totals);

            await processor.Process(invoice);

            // TODO: assert totals
            Assert.NotNull(invoice.Totals);

            Assert.Equal(expectedNumber, invoice.Number);

            Assert.Equal(1, invoice.Sequence);

            // TODO: assert signature
            Assert.NotNull(invoice.Signature);

            store.Verify(x => x.Store(It.Is<Invoice>(i => i == invoice), expectedBucket));
        }
    }
}