using System;
using System.Threading.Tasks;
using Bogus;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using Vera.Grpc;
using Vera.Integration.Tests.Common;
using Xunit;

namespace Vera.Integration.Tests.Portugal
{
    public class AuditServiceTests : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly GrpcChannel _channel;
        private readonly Faker _faker;

        public AuditServiceTests(ApiWebApplicationFactory fixture)
        {
            var client = fixture.CreateClient();

            _channel = GrpcChannel.ForAddress(client.BaseAddress!, new GrpcChannelOptions
            {
                HttpClient = client
            });

            _faker = new Faker();
        }

        [Fact]
        public async Task Should_be_able_to_archive_an_audit()
        {
            var setup = new Setup(_channel, _faker);
            var token = await setup.CreateLogin();
            var account = await setup.CreateAccount();

            var portugalSetup = new PortugalSetup(setup);
            await portugalSetup.ConfigureAccount(account);

            var invoiceGenerator = new InvoiceGenerator(_faker);
            var invoice = invoiceGenerator.CreateInvoiceWithCustomerAndSingleProduct(account);

            var createInvoiceRequest = new CreateInvoiceRequest
            {
                Invoice = invoice
            };

            var invoiceService = new InvoiceService.InvoiceServiceClient(_channel);
            using var createInvoiceCall = invoiceService.CreateAsync(createInvoiceRequest, setup.CreateAuthorizedMetadata());

            var createAuditRequest = new CreateAuditRequest
            {
                AccountId = account,
                SupplierSystemId = invoice.Supplier.SystemId,
                StartDate = DateTime.UtcNow.AddDays(-1).ToTimestamp(),
                EndDate = DateTime.UtcNow.AddDays(1).ToTimestamp()
            };

            var auditService = new AuditService.AuditServiceClient(_channel);
            using var createAuditCall = auditService.CreateAsync(
                createAuditRequest,
                setup.CreateAuthorizedMetadata()
            );

            var createAuditReply = await createAuditCall.ResponseAsync;

            Assert.NotNull(createAuditReply.AuditId);

            var reply = new GetAuditReply();
            for (var i = 0; i < 5; i++)
            {
                // Wait a little before the audit is finished
                await Task.Delay(100);

                var getAuditRequest = new GetAuditRequest
                {
                    AccountId = account,
                    AuditId = createAuditReply.AuditId
                };

                using var getAuditCall = auditService.GetAsync(getAuditRequest, setup.CreateAuthorizedMetadata());

                reply = await getAuditCall.ResponseAsync;

                if (!string.IsNullOrEmpty(reply.Location))
                {
                    break;
                }
            }

            Assert.False(string.IsNullOrEmpty(reply.Location));
        }
    }
}