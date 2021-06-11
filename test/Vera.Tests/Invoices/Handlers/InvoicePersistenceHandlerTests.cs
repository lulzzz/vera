using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Text;
using System.Threading.Tasks;
using Vera.Dependencies;
using Vera.Invoices;
using Vera.Models;
using Vera.Signing;
using Vera.Stores;
using Xunit;

namespace Vera.Tests.Invoices.Handlers
{
    public class InvoicePersistenceHandlerTests
    {
        const string expectedBucket = "my-bucket";
        const string expectedNumber = "my-invoice-no";
        const string supplierSystemId = "A1";

        private readonly byte[] signature = Encoding.UTF8.GetBytes("signature");
        private readonly Supplier supplier;
        private readonly Guid registerId;

        private readonly Mock<IChainable> last;
        private readonly Mock<IInvoiceStore> invoiceStore;
        private readonly Mock<IChainStore> chainStore;
        private readonly Mock<ILogger<InvoicePersistenceHandler>> logger;
        private readonly Mock<IBucketGenerator<Invoice>> bucketGenerator;
        private readonly Mock<IInvoiceNumberGenerator> invoiceNumberGenerator;
        private readonly Mock<IInvoiceSigner> signer;
        private readonly InvoicePersistenceHandler persistanceHandler;
        private readonly Mock<IGrandTotalAuditTrailStore> grandTotalAuditTrailStore;

        public InvoicePersistenceHandlerTests()
        {
            supplier = new Supplier { SystemId = supplierSystemId };
            registerId = Guid.NewGuid();
;
            last = new Mock<IChainable>();
            invoiceStore = new Mock<IInvoiceStore>();
            chainStore = new Mock<IChainStore>();
            logger = new Mock<ILogger<InvoicePersistenceHandler>>();
            bucketGenerator = new Mock<IBucketGenerator<Invoice>>();
            invoiceNumberGenerator = new Mock<IInvoiceNumberGenerator>();
            signer = new Mock<IInvoiceSigner>();
            grandTotalAuditTrailStore = new Mock<IGrandTotalAuditTrailStore>();

            bucketGenerator.Setup(x => x.Generate(It.IsAny<Invoice>()))
                .Returns(expectedBucket);

            invoiceNumberGenerator.Setup(x => x.Generate(It.IsAny<Invoice>()))
                .ReturnsAsync(expectedNumber);

            signer.Setup(x => x.Sign(It.IsAny<Invoice>(), It.IsAny<Signature>()))
                .ReturnsAsync(new Signature { Output = signature });

            last.SetupGet(x => x.NextSequence)
                .Returns(1);

            chainStore.Setup(x => x.Last(It.IsAny<ChainContext>()))
                .ReturnsAsync(last.Object);

            persistanceHandler = new InvoicePersistenceHandler(
                logger.Object,
                chainStore.Object,
                invoiceStore.Object,
                signer.Object,
                invoiceNumberGenerator.Object,
                bucketGenerator.Object,
                bucketGenerator.Object,
                grandTotalAuditTrailStore.Object
            );
        }

        [Fact]
        public async Task Should_invoke_handler()
        {
            var invoice = new Invoice
            {
                RegisterSystemId = registerId.ToString(),
                Supplier = supplier
            };

            await persistanceHandler.Handle(invoice);

            Assert.Equal(last.Object.NextSequence, invoice.Sequence);
            Assert.Equal(expectedNumber, invoice.Number);
            Assert.Equal(signature, invoice.Signature.Output);

            invoiceStore.Verify(s => s.Store(invoice));

            last.Verify(l => l.Append(invoice.Signature, It.IsAny<decimal>()));
        }

        [Fact]
        public async Task Should_delete_on_error()
        {
            var invoice = new Invoice
            {
                RegisterSystemId = registerId.ToString(),
                Supplier = supplier
            };

            last.Setup(x => x.Append(It.IsAny<Signature>(), It.IsAny<decimal>()))
                .ThrowsAsync(new Exception());

            await persistanceHandler.Handle(invoice);

            invoiceStore.Verify(l => l.Delete(invoice));
        }

        [Fact]
        public async Task Should_persist_invoice_and_invoke_next()
        {
            var invoice = new Invoice
            {
                RegisterSystemId = registerId.ToString(),
                Supplier = supplier
            };

            await persistanceHandler.Handle(invoice);
            var mockHandler = new InvoiceHandlersHelper().MockInvoiceHandler;
            persistanceHandler.WithNext(mockHandler.Object);

            await persistanceHandler.Handle(invoice);

            Assert.Equal(expectedNumber, invoice.Number);

            mockHandler.Verify(h => h.Handle(invoice));
        }
    }
}
