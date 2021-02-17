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
    public class AuditArchiverTests
    {
        [Fact]
        public async Task Should_throw_when_start_is_before_end()
        {
            var archiver = new AuditArchiver(null, null, null, null);

            await Assert.ThrowsAsync<InvalidOperationException>(() => archiver.Archive(new Account(), new Audit
            {
                Start = new DateTime(2020, 01, 01),
                End = new DateTime(2019, 01, 01)
            }));
        }

        [Fact]
        public async Task Should_invoke_writer_multiple_times()
        {
            var invoiceStore = new Mock<IInvoiceStore>();
            var blobStore = new Mock<IBlobStore>();
            var auditStore = new Mock<IAuditStore>();
            var writer = new Mock<IAuditWriter>();

            invoiceStore
                .Setup(store => store.List(It.IsAny<AuditCriteria>()))
                .ReturnsAsync(new List<Invoice>
                {
                    new()
                });

            writer
                .Setup(w => w.ResolveName(It.IsAny<AuditCriteria>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(() => "abc.xml");

            var archiver = new AuditArchiver(
                invoiceStore.Object,
                blobStore.Object,
                auditStore.Object,
                writer.Object
            );

            var account = new Account();
            var audit = new Audit
            {
                Start = new DateTime(2020, 1, 1),
                End = new DateTime(2021, 1, 1)
            };

            await archiver.Archive(account, audit);

            writer.Verify(
                w => w.Write(It.IsAny<AuditContext>(), It.IsAny<AuditCriteria>(), It.IsAny<Stream>()),
                Times.Exactly(12)
            );
        }
    }
}