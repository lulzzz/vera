using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Vera.Concurrency;
using Vera.Dependencies;
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

            var invoiceStore = new Mock<IInvoiceStore>();
            var chainStore = new Mock<IChainStore>();
            var last = new Mock<IChainable>();
            var factory = new Mock<IInvoiceComponentFactory>();
            var supplierStore = new Mock<ISupplierStore>();
            var periodStore = new Mock<IPeriodStore>();

            var bucketGenerator = new Mock<IBucketGenerator<Invoice>>();
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

            supplierStore.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(new Supplier());

            var registerId = Guid.NewGuid();
            var period = new Period { Registers = { new Register { Id = registerId } }};
            periodStore.Setup(x => x.GetOpenPeriodForSupplier(It.IsAny<Guid>()))
                .ReturnsAsync(period);

            var invoiceHandlerFactory = new InvoiceHandlerFactory(
                NullLoggerFactory.Instance,
                invoiceStore.Object,
                chainStore.Object,
                new InMemoryLocker(),
                supplierStore.Object,
                periodStore.Object
            );

            var builder = new InvoiceBuilder();
            var director = new InvoiceDirector(builder, Guid.Empty, "1");
            director.ConstructAnonymousWithSingleProductPaidWithCash();

            var invoice = builder.Result;
            invoice.RegisterId = registerId.ToString();

            await invoiceHandlerFactory.Create(factory.Object).Handle(invoice);

            Assert.Equal(expectedNumber, invoice.Number);
            Assert.Equal(last.Object.NextSequence, invoice.Sequence);

            // TODO: assert signature
            Assert.NotNull(invoice.Signature);
        }
    }
}