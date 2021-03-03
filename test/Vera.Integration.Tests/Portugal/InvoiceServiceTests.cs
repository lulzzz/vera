using System;
using System.Threading.Tasks;
using Bogus;
using Vera.Grpc;
using Vera.Grpc.Models;
using Vera.Models;
using Vera.Tests.Shared;
using Xunit;
using Product = Vera.Models.Product;

namespace Vera.Integration.Tests.Portugal
{
    public class InvoiceServiceTests : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly Setup _setup;
        private readonly InvoiceBuilder _invoiceBuilder;
        private readonly InvoiceDirector _invoiceDirector;

        public InvoiceServiceTests(ApiWebApplicationFactory fixture)
        {
            _setup = fixture.CreateSetup();
            _invoiceBuilder = new InvoiceBuilder(new Faker());
            _invoiceDirector = new InvoiceDirector(_invoiceBuilder);
        }

        [Fact]
        public async Task Should_be_able_to_create_an_invoice()
        {
            var client = await _setup.CreateClient(Constants.Account);
            
            var invoice = _invoiceDirector.CreateAnonymousSingleProductPaidWithCash(Guid.Parse(client.AccountId));

            var createInvoiceRequest = new CreateInvoiceRequest
            {
                Invoice = invoice.Pack()
            };

            var createInvoiceReply = await client.Invoice.CreateAsync(createInvoiceRequest, client.AuthorizedMetadata);

            // TODO(kevin): need more of these scenarios
            Assert.Equal($"itFR {invoice.Supplier.SystemId}/{createInvoiceReply.Sequence}", createInvoiceReply.Number);
            Assert.True(createInvoiceReply.Sequence > 0);
        }

        [Fact]
        public async Task Should_be_able_to_create_an_invoice_with_multiple_payments()
        {
            var client = await _setup.CreateClient(Constants.Account);
            
            _invoiceDirector.CreateEmptyAnonymous(Guid.Parse(client.AccountId));

            var invoice = _invoiceBuilder
                .WithPayment(50m, PaymentCategory.Debit)
                .WithPayment(50m, PaymentCategory.Cash)
                .WithAmount(100m, 1.23m)
                .Build();
            
            var createInvoiceRequest = new CreateInvoiceRequest
            {
                Invoice = invoice.Pack()
            };

            var createInvoiceReply = await client.Invoice.CreateAsync(createInvoiceRequest, client.AuthorizedMetadata);

            Assert.Contains("FR", createInvoiceReply.Number);
        }

        [Fact]
        public async Task Should_have_an_ascending_sequence()
        {
            var client = await _setup.CreateClient(Constants.Account);

            var accountId = Guid.Parse(client.AccountId);
            var firstInvoice = _invoiceDirector.CreateAnonymousSingleProductPaidWithCash(accountId);
            var nextInvoice = _invoiceDirector.CreateAnonymousSingleProductPaidWithCash(accountId);

            // Make sure both invoices are from the same supplier so they land in the same chain
            firstInvoice.Supplier.SystemId = nextInvoice.Supplier.SystemId = 1.ToString();

            var first = await client.Invoice.CreateAsync(new CreateInvoiceRequest
            {
                Invoice = firstInvoice.Pack()
            }, client.AuthorizedMetadata);

            var next = await client.Invoice.CreateAsync(new CreateInvoiceRequest
            {
                Invoice = nextInvoice.Pack()
            }, client.AuthorizedMetadata);

            Assert.True(first.Sequence < next.Sequence, $"{first.Sequence} < {next.Sequence}");
        }
        
        // TODO(kevin): create test that directly attempts to create an invoice with validation errors
        // TODO(kevin): write tests that generate different invoices and verify that the number matches the expected format

        [Fact]
        public async Task Should_be_able_to_run_validation()
        {
            var client = await _setup.CreateClient(Constants.Account);

            var invoice = _invoiceDirector.CreateAnonymousSingleProductPaidWithCash(Guid.Parse(client.AccountId));

            var validationReply = await client.Invoice.ValidateAsync(new ValidateInvoiceRequest
            {
                AccountId = client.AccountId,
                Invoice = invoice.Pack()
            }, client.AuthorizedMetadata);
            
            Assert.Empty(validationReply.Results);
        }

        [Fact]
        public async Task Should_be_able_to_get_validation_results()
        {
            var client = await _setup.CreateClient(Constants.Account);

            var invoice = _invoiceDirector.CreateAnonymousSingleProductPaidWithCash(Guid.Parse(client.AccountId));
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