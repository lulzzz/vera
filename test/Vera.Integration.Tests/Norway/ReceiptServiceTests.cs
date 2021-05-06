using System;
using System.Threading.Tasks;
using Vera.Grpc;
using Vera.Host.Mapping;
using Vera.Tests.Shared;
using Xunit;

namespace Vera.Integration.Tests.Norway
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
            
            var builder = new InvoiceBuilder();
            var director = new InvoiceDirector(builder, Guid.Parse(client.AccountId), client.SupplierSystemId);
            director.ConstructAnonymousWithSingleProductPaidWithCash();

            await client.OpenPeriod();
            var openRegisterReply = await client.OpenRegister(100m);

            var invoice = builder.Result;
            invoice.RegisterId = openRegisterReply.Id;

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

            var receiptContent = renderReceiptReply.Content.ToStringUtf8();

            Assert.Contains("Salgskvittering", receiptContent);
            Assert.Contains("Foretaksregisteret", receiptContent);
            Assert.Contains($"Kvitteringnummer: {invoice.Number}", receiptContent);
            Assert.Contains("OPPRINNELIG", receiptContent);
        }

        [Fact]
        public async Task Should_be_able_to_mark_receipt_as_printed()
        {
            var client = await _setup.CreateClient(Constants.Account);

            var builder = new InvoiceBuilder();
            var director = new InvoiceDirector(builder, Guid.Parse(client.AccountId), client.SupplierSystemId);
            director.ConstructAnonymousWithSingleProductPaidWithCash();

            await client.OpenPeriod();
            var openRegisterReply = await client.OpenRegister(100m);

            var invoice = builder.Result;
            invoice.RegisterId = openRegisterReply.Id;


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