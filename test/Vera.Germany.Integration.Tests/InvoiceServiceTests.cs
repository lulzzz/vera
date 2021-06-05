using System;
using System.Threading.Tasks;
using Vera.Grpc;
using Vera.Host.Mapping;
using Vera.Integration.Tests;
using Vera.Tests.Shared;
using Xunit;

namespace Vera.Germany.Integration.Tests
{
    public class InvoiceServiceTests : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly Setup _setup;
        private readonly ApiWebApplicationFactory _fixture;

        public InvoiceServiceTests(ApiWebApplicationFactory fixture)
        {
            _fixture = fixture;
            _setup = _fixture.CreateSetup();
        }

        [Fact(Skip = "Lacks correct configuration")]
        public async Task Should_be_able_to_create_an_invoice()
        {
            var client = await _setup.CreateClient(Constants.Account);

            var builder = new InvoiceBuilder();
            var director = new InvoiceDirector(builder, Guid.Parse(client.AccountId), client.SupplierSystemId);
            director.ConstructAnonymousWithSingleProductPaidWithCash();

            await client.OpenPeriod();
            var registerSystemId = await client.OpenRegister(100m);

            var invoice = builder.Result;
            invoice.RegisterSystemId = registerSystemId;
            var createInvoiceRequest = new CreateInvoiceRequest
            {
                Invoice = invoice.Pack()
            };

            var createInvoiceReply = await client.Invoice.CreateAsync(createInvoiceRequest, client.AuthorizedMetadata);

            Assert.True(createInvoiceReply.Sequence > 0);
        }

    }
}
