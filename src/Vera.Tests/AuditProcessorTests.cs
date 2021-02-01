using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Moq;
using Vera.Audits;
using Vera.Models;
using Vera.Stores;
using Xunit;

namespace Vera.Tests
{
    public class AuditProcessorTests
    {
        [Fact]
        public async Task Should_process()
        {
            const string expectedLocation = "my-static-location";

            var invoiceStore = new Mock<IInvoiceStore>();
            var blobStore = new Mock<IBlobStore>();
            var auditStore = new Mock<IAuditStore>();
            var factory = new Mock<IComponentFactory>();

            invoiceStore
                .Setup(store => store.List(It.IsAny<AuditCriteria>()))
                .ReturnsAsync(new List<Invoice>());

            blobStore
                .Setup(store => store.Store(It.IsAny<Guid>(), It.IsAny<Stream>()))
                .ReturnsAsync(expectedLocation);

            factory
                .Setup(f => f.CreateAuditWriter())
                .Returns(new Mock<IAuditWriter>().Object);

            var processor = new AuditProcessor(
                invoiceStore.Object,
                blobStore.Object,
                auditStore.Object,
                factory.Object
            );

            var account = new Account();
            var audit = new Audit();

            await processor.Process(account, audit);

            Assert.Equal(expectedLocation, audit.Location);
        }
    }
}