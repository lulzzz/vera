using System.Threading.Tasks;
using Bogus;
using Grpc.Net.Client;
using Vera.Grpc;
using Vera.Grpc.Models;
using Vera.Integration.Tests.Common;
using Vera.Tests.Shared;
using Xunit;

namespace Vera.Integration.Tests.Portugal
{
    public class ReceiptServiceTests : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly Setup _setup;

        public ReceiptServiceTests(ApiWebApplicationFactory fixture)
        {
            _setup = fixture.CreateSetup();
        }

        [Fact]
        public async Task Should_be_able_to_generate_a_receipt()
        {
            var client = await _setup.CreateClient(Constants.Account);

            var invoiceGenerator = new InvoiceGenerator(new Faker());
            var invoice = invoiceGenerator.CreateWithCustomerAndSingleProduct(client.AccountId);

            var createInvoiceRequest = new CreateInvoiceRequest
            {
                Invoice = invoice.Pack()
            };

            var createInvoiceReply = await client.Invoice.CreateAsync(createInvoiceRequest, client.AuthorizedMetadata);

            var renderReceiptReply = await client.Receipt.RenderThermalAsync(new RenderThermalRequest
            {
                AccountId = client.AccountId,
                InvoiceNumber = createInvoiceReply.Number,
                Type = ReceiptOutputType.Json
            }, client.AuthorizedMetadata);

            Assert.Equal(ReceiptOutputType.Json, renderReceiptReply.Type);
            Assert.NotNull(renderReceiptReply.Content);

            // TODO(kevin): verify receipt content - important stuff that needs to be on there
            var receiptContent = renderReceiptReply.Content.ToStringUtf8();
        }

        [Fact]
        public async Task Should_be_able_to_mark_receipt_as_printed()
        {
            var client = await _setup.CreateClient(Constants.Account);

            var invoiceGenerator = new InvoiceGenerator(new Faker());
            var invoice = invoiceGenerator.CreateWithCustomerAndSingleProduct(client.AccountId);

            var createInvoiceRequest = new CreateInvoiceRequest
            {
                Invoice = invoice.Pack()
            };

            var createInvoiceReply = await client.Invoice.CreateAsync(createInvoiceRequest, client.AuthorizedMetadata);

            var renderReceiptReply = await client.Receipt.RenderThermalAsync(new RenderThermalRequest
            {
                AccountId = client.AccountId,
                InvoiceNumber = createInvoiceReply.Number,
                Type = ReceiptOutputType.Json
            }, client.AuthorizedMetadata);

            await client.Receipt.UpdatePrintResultAsync(new UpdatePrintResultRequest
            {
                AccountId = client.AccountId,
                Token = renderReceiptReply.Token,
                Success = true,
            }, client.AuthorizedMetadata);
        }
    }
}