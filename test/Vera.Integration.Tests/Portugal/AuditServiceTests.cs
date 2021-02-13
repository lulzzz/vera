using System;
using System.Threading.Tasks;
using Bogus;
using Google.Protobuf.WellKnownTypes;
using Vera.Grpc;
using Vera.Grpc.Models;
using Vera.Tests.Shared;
using Xunit;

namespace Vera.Integration.Tests.Portugal
{
    public class AuditServiceTests : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly Setup _setup;

        public AuditServiceTests(ApiWebApplicationFactory fixture)
        {
            _setup = fixture.CreateSetup();
        }

        [Fact]
        public async Task Should_be_able_to_archive_an_audit()
        {
            var client = await _setup.CreateClient(Constants.Account);

            var invoiceGenerator = new InvoiceGenerator(new Faker());
            var invoice = invoiceGenerator.CreateWithCustomerAndSingleProduct(client.AccountId);

            var createInvoiceRequest = new CreateInvoiceRequest
            {
                Invoice = invoice.Pack()
            };

            await client.Invoice.CreateAsync(createInvoiceRequest, client.AuthorizedMetadata);

            var createAuditRequest = new CreateAuditRequest
            {
                AccountId = client.AccountId,
                SupplierSystemId = invoice.Supplier.SystemId,
                StartDate = DateTime.UtcNow.AddDays(-1).ToTimestamp(),
                EndDate = DateTime.UtcNow.AddDays(1).ToTimestamp()
            };

            var createAuditReply = await client.Audit.CreateAsync(createAuditRequest, client.AuthorizedMetadata);

            Assert.NotNull(createAuditReply.AuditId);

            var reply = new GetAuditReply();
            for (var i = 0; i < 10; i++)
            {
                // Wait a little before the audit is finished
                await Task.Delay(100);

                var getAuditRequest = new GetAuditRequest
                {
                    AccountId = client.AccountId,
                    AuditId = createAuditReply.AuditId
                };

                using var getAuditCall = client.Audit.GetAsync(getAuditRequest, client.AuthorizedMetadata);

                reply = await getAuditCall.ResponseAsync;

                if (!string.IsNullOrEmpty(reply.Location))
                {
                    break;
                }
            }
            
            // TODO(kevin): get the file and check contents?

            Assert.False(string.IsNullOrEmpty(reply.Location));
        }
    }
}