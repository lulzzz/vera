using System.Threading.Tasks;
using Bogus;
using Grpc.Net.Client;
using Vera.Grpc;
using Vera.Integration.Tests.Common;
using Xunit;

namespace Vera.Integration.Tests.Portugal
{
    public class ReceiptServiceTests : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly GrpcChannel _channel;
        private readonly Faker _faker;

        public ReceiptServiceTests(ApiWebApplicationFactory fixture)
        {
            var client = fixture.CreateClient();

            _channel = GrpcChannel.ForAddress(client.BaseAddress!, new GrpcChannelOptions
            {
                HttpClient = client
            });

            _faker = new Faker();
        }

        [Fact]
        public async Task Should_be_able_to_generate_a_receipt()
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

            var createInvoiceReply = await createInvoiceCall.ResponseAsync;

            var receiptService = new ReceiptService.ReceiptServiceClient(_channel);
            using var renderReceiptCall = receiptService.RenderThermalAsync(new RenderThermalRequest
            {
                Account = account,
                Number = createInvoiceReply.Number,
                Type = ReceiptOutputType.Json
            }, setup.CreateAuthorizedMetadata());

            var renderReceiptReply = await renderReceiptCall.ResponseAsync;

            Assert.Equal(ReceiptOutputType.Json, renderReceiptReply.Type);
            Assert.NotNull(renderReceiptReply.Content);

            var receiptContent = renderReceiptReply.Content.ToStringUtf8();
        }
    }
}