using System;
using System.Threading.Tasks;
using Bogus;
using Grpc.Net.Client;
using Vera.Grpc;
using Vera.Integration.Tests.Common;
using Xunit;

namespace Vera.Integration.Tests.Portugal
{
    public class InvoiceServiceTests : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly GrpcChannel _channel;
        private readonly Faker _faker;

        public InvoiceServiceTests(ApiWebApplicationFactory fixture)
        {
            var client = fixture.CreateClient();

            _channel = GrpcChannel.ForAddress(client.BaseAddress!, new GrpcChannelOptions
            {
                HttpClient = client
            });

            _faker = new Faker();
        }

        [Fact]
        public async Task Should_be_able_to_create_an_invoice()
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

            Assert.NotNull(createInvoiceReply.Number);
            Assert.NotNull(createInvoiceReply.Signature);

            using var getInvoiceCall = invoiceService.GetByNumberAsync(new GetInvoiceByNumberRequest
            {
                AccountId = account,
                Number = createInvoiceReply.Number
            }, setup.CreateAuthorizedMetadata());

            var getInvoiceCallReply = await getInvoiceCall.ResponseAsync;

            // TODO(kevin): verify

            Assert.Equal(createInvoiceReply.Number, getInvoiceCallReply.Number);
        }

        // TODO(kevin): write tests that generate different invoices and verify that the number matches the expected format
    }
}