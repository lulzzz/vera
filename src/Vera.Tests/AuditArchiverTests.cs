using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Vera.Audits;
using Vera.Models;
using Vera.Stores;
using Xunit;

namespace Vera.Tests
{
    public class AuditArchiverTests
    {
        [Fact]
        public async Task Should_process()
        {
            const string expectedLocation = "my-static-location";

            var invoiceStore = new Mock<IInvoiceStore>();
            var blobStore = new Mock<IBlobStore>();
            var auditStore = new Mock<IAuditStore>();
            var factory = new Mock<IComponentFactory>();
            var writer = new Mock<IAuditWriter>();

            invoiceStore
                .Setup(store => store.List(It.IsAny<AuditCriteria>()))
                .ReturnsAsync(new List<Invoice>
                {
                    new()
                });

            blobStore
                .Setup(store => store.Store(It.IsAny<Guid>(), It.IsAny<Stream>()))
                .ReturnsAsync(expectedLocation);

            factory
                .Setup(f => f.CreateAuditWriter())
                .Returns(writer.Object);

            var processor = new AuditArchiver(
                invoiceStore.Object,
                blobStore.Object,
                auditStore.Object,
                factory.Object
            );

            var account = new Account();
            var audit = new Audit
            {
                Start = new DateTime(2020, 1, 1),
                End = new DateTime(2021, 1, 1)
            };

            await processor.Archive(account, audit);

            Assert.Equal(expectedLocation, audit.Location);
        }
    }
}