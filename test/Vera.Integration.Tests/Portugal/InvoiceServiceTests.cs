using System;
using System.Threading.Tasks;
using Bogus;
using Vera.Grpc;
using Vera.Grpc.Models;
using Vera.Models;
using Vera.Tests.Shared;
using Xunit;

namespace Vera.Integration.Tests.Portugal
{
    public class InvoiceServiceTests : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly Setup _setup;

        public InvoiceServiceTests(ApiWebApplicationFactory fixture)
        {
            _setup = fixture.CreateSetup();
        }

        [Fact]
        public async Task Should_be_able_to_create_an_invoice()
        {
            var client = await _setup.CreateClient(Constants.Account);

            var builder = new InvoiceBuilder();
            var director = new InvoiceDirector(builder, Guid.Parse(client.AccountId), "1");
            director.ConstructAnonymousWithSingleProductPaidWithCash();

            var invoice = builder.Result;
            
            var createInvoiceRequest = new CreateInvoiceRequest
            {
                Invoice = invoice.Pack()
            };

            var createInvoiceReply = await client.Invoice.CreateAsync(createInvoiceRequest, client.AuthorizedMetadata);

            Assert.Equal($"itFR {invoice.Supplier.SystemId}/{createInvoiceReply.Sequence}", createInvoiceReply.Number);
            Assert.True(createInvoiceReply.Sequence > 0);
        }

        [Fact]
        public async Task Should_have_an_ascending_sequence()
        {
            var client = await _setup.CreateClient(Constants.Account);
            
            var builder = new InvoiceBuilder();
            var director = new InvoiceDirector(builder, Guid.Parse(client.AccountId), "1");
            director.ConstructAnonymousWithSingleProductPaidWithCash();

            var invoice = builder.Result;

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

            var builder = new InvoiceBuilder();
            var director = new InvoiceDirector(builder, Guid.Parse(client.AccountId), "1");
            director.ConstructAnonymousWithSingleProductPaidWithCash();
            
            var validationReply = await client.Invoice.ValidateAsync(new ValidateInvoiceRequest
            {
                AccountId = client.AccountId,
                Invoice = builder.Result.Pack()
            }, client.AuthorizedMetadata);
            
            Assert.Empty(validationReply.Results);
        }

        [Fact]
        public async Task Should_be_able_to_get_validation_results()
        {
            var client = await _setup.CreateClient(Constants.Account);
            
            var builder = new InvoiceBuilder();
            var director = new InvoiceDirector(builder, Guid.Parse(client.AccountId), "1");
            director.ConstructAnonymousWithSingleProductPaidWithCash();

            var invoice = builder.Result;
            
            invoice.Lines.Add(new Models.InvoiceLine
            {
                Description = "trigger mixed quantities",
                Product = new()
                {
                    Code = "1234",
                    Description = "trigger mixed quantities",
                    Type = ProductType.Goods
                },
                Quantity = -1,
                Taxes = new Taxes
                {
                    Category = TaxesCategory.High,
                    Rate = 1.23m,
                    Code = "IVA"
                },
            });

            var validationReply = await client.Invoice.ValidateAsync(new ValidateInvoiceRequest
            {
                AccountId = client.AccountId,
                Invoice = invoice.Pack()
            }, client.AuthorizedMetadata);

            Assert.Contains(validationReply.Results, x => x.Key == "Lines");
        }
    }
}