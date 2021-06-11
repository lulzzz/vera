using System;
using System.Threading.Tasks;
using Vera.Grpc;
using Vera.Host.Mapping;
using Vera.Integration.Tests;
using Vera.Tests.Scenario;
using Vera.Tests.Shared;
using Xunit;

namespace Vera.Norway.Integration.Tests
{
    public class InvoiceServiceTests : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly Setup _setup;

        public InvoiceServiceTests(ApiWebApplicationFactory fixture)
        {
            _setup = fixture.CreateSetup();
        }

        /*[Fact]
        public async Task Should_create_multiple_invoices_and_their_grand_totals_and_chain_items()
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
            createInvoiceReply = await client.Invoice.CreateAsync(createInvoiceRequest, client.AuthorizedMetadata);
            createInvoiceReply = await client.Invoice.CreateAsync(createInvoiceRequest, client.AuthorizedMetadata);
        }*/

        [Fact]
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

            //TODO construct number according to the docs
            Assert.Equal($"{createInvoiceReply.Sequence}", createInvoiceReply.Number);
            Assert.True(createInvoiceReply.Sequence > 0);
        }

        [Fact]
        public async Task Should_have_an_ascending_sequence()
        {
            var client = await _setup.CreateClient(Constants.Account);

            var builder = new InvoiceBuilder();
            var director = new InvoiceDirector(builder, Guid.Parse(client.AccountId), client.SupplierSystemId);
            director.ConstructAnonymousWithSingleProductPaidWithCash();

            await client.OpenPeriod();
            var registerSystemId = await client.OpenRegister(100m);

            var invoice = builder.Result;
            invoice.RegisterSystemId = registerSystemId;

            // Create same transaction twice to verify sequence is incremented
            var first = await client.Invoice.CreateAsync(new CreateInvoiceRequest
            {
                Invoice = invoice.Pack()
            }, client.AuthorizedMetadata);

            var next = await client.Invoice.CreateAsync(new CreateInvoiceRequest
            {
                Invoice = invoice.Pack()
            }, client.AuthorizedMetadata);

            Assert.True(first.Sequence < next.Sequence, $"{first.Sequence} < {next.Sequence}");
        }

        [Fact]
        public async Task Should_be_able_to_run_validation()
        {
            var client = await _setup.CreateClient(Constants.Account);

            var scenario = new SellProductScenario(ProductFactory.CreateRandomProduct())
            {
                AccountId = Guid.Parse(client.AccountId),
                SupplierSystemId = client.SupplierSystemId
            };

            var result = scenario.Execute();
            var invoice = result.Invoice;

            var validationReply = await client.Invoice.ValidateAsync(new ValidateInvoiceRequest
            {
                AccountId = client.AccountId,
                Invoice = invoice.Pack()
            }, client.AuthorizedMetadata);

            Assert.Empty(validationReply.Results);
        }

        [Fact]
        public async Task Should_create_invoice_with_supplier()
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

            var getByNumberRequest = new GetInvoiceByNumberRequest
            {
                AccountId = client.AccountId,
                Number = createInvoiceReply.Number
            };

            var getInvoiceReply = client.Invoice.GetByNumber(getByNumberRequest, client.AuthorizedMetadata);

            Assert.Equal(client.SupplierSystemId, getInvoiceReply.Supplier.SystemId);
        }

        [Fact]
        public async Task Should_create_invoice_with_period()
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

            var getByNumberRequest = new GetInvoiceByNumberRequest
            {
                AccountId = client.AccountId,
                Number = createInvoiceReply.Number
            };

            var getInvoiceReply = client.Invoice.GetByNumber(getByNumberRequest, client.AuthorizedMetadata);

            var getPeriodRequest = new GetCurrentPeriodRequest { SupplierSystemId = client.SupplierSystemId };
            var getCurrentPeriodReply = await client.Period.GetCurrentPeriodAsync(getPeriodRequest, client.AuthorizedMetadata);

            Assert.Equal(getCurrentPeriodReply.Id, getInvoiceReply.PeriodId);
        }
    }
}
