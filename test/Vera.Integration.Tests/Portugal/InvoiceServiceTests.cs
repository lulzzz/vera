using System.Diagnostics;
using System.Threading.Tasks;
using Bogus;
using Vera.Grpc;
using Vera.Grpc.Models;
using Vera.Tests.Shared;
using Xunit;

namespace Vera.Integration.Tests.Portugal
{
    public class InvoiceServiceTests : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly Setup _setup;
        private readonly InvoiceGenerator _invoiceGenerator;

        public InvoiceServiceTests(ApiWebApplicationFactory fixture)
        {
            _setup = fixture.CreateSetup();
            _invoiceGenerator = new InvoiceGenerator(new Faker());
        }

        [Fact]
        public async Task Should_be_able_to_create_an_invoice()
        {
            var client = await _setup.CreateClient(Constants.Account);
            
            var invoice = _invoiceGenerator.CreateWithCustomerAndSingleProduct(client.AccountId);

            var createInvoiceRequest = new CreateInvoiceRequest
            {
                Invoice = invoice.Pack()
            };

            var createInvoiceReply = await client.Invoice.CreateAsync(createInvoiceRequest, client.AuthorizedMetadata);

            // TODO(kevin): need more of these scenarios
            Assert.Equal($"itFS {invoice.Supplier.SystemId}/{createInvoiceReply.Sequence}", createInvoiceReply.Number);
            Assert.True(createInvoiceReply.Sequence > 0);
        }

        [Fact]
        public async Task Should_have_an_ascending_sequence()
        {
            var client = await _setup.CreateClient(Constants.Account);

            var firstInvoice = _invoiceGenerator.CreateWithCustomerAndSingleProduct(client.AccountId);
            var nextInvoice = _invoiceGenerator.CreateWithCustomerAndSingleProduct(client.AccountId);

            var firstInvoiceReply = client.Invoice.CreateAsync(new CreateInvoiceRequest
            {
                Invoice = firstInvoice.Pack()
            }, client.AuthorizedMetadata);

            var nextInvoiceReply = client.Invoice.CreateAsync(new CreateInvoiceRequest
            {
                Invoice = nextInvoice.Pack()
            }, client.AuthorizedMetadata);

            await Task.WhenAll(firstInvoiceReply.ResponseAsync, nextInvoiceReply.ResponseAsync);

            var first = firstInvoiceReply.ResponseAsync.Result;
            var next = nextInvoiceReply.ResponseAsync.Result;
            
            // TODO(kevin): generates same sequence if run with other portugal test
            Assert.True(first.Sequence < next.Sequence, $"{first.Sequence} < {next.Sequence}");
        }
        
        // TODO(kevin): write tests that generate different invoices and verify that the number matches the expected format
    }
}