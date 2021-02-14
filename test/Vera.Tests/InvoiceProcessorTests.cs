using System;
using System.Threading.Tasks;
using Bogus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
            var chainStore = new Mock<IChainStore>();
            var last = new Mock<IChainable>();
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

            factory.Setup(f => f.CreateInvoiceValidator())
                .Returns(new NullInvoiceValidator());
            
            bucketGenerator.Setup(x => x.Generate(It.IsAny<Invoice>()))
                .Returns(expectedBucket);

            invoiceNumberGenerator.Setup(x => x.Generate(It.IsAny<Invoice>()))
                .ReturnsAsync(expectedNumber);

            signer.Setup(x => x.Sign(It.IsAny<Package>()))
                .ReturnsAsync(new Signature());

            last.SetupGet(x => x.NextSequence)
                .Returns(1);
            
            chainStore.Setup(x => x.Last(It.IsAny<ChainContext>()))
                .ReturnsAsync(last.Object);

            var processor = new InvoiceProcessor(
                new NullLogger<InvoiceProcessor>(),
                store.Object,
                chainStore.Object,
                new InMemoryLocker()
            );

            var invoiceGenerator = new InvoiceGenerator(new Faker());
            var invoice = invoiceGenerator.CreateAnonymousWithSingleProduct(Guid.Empty.ToString());

            await processor.Process(factory.Object, invoice);

            Assert.Equal(expectedNumber, invoice.Number);
            Assert.Equal(last.Object.NextSequence, invoice.Sequence);

            // TODO: assert signature
            Assert.NotNull(invoice.Signature);

            // store.Verify(x => x.Store(It.Is<Invoice>(i => i == invoice), expectedBucket));
        }
    }
}