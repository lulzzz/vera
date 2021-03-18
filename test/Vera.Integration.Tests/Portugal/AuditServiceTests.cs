using System;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Google.Protobuf.WellKnownTypes;
using Vera.Grpc;
using Vera.Grpc.Models;
using Vera.Models;
using Vera.Tests.Shared;
using Xunit;

namespace Vera.Integration.Tests.Portugal
{
    public class AuditServiceTests : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly Setup _setup;
        private readonly ApiWebApplicationFactory _fixture;
        private AuditResultsReader _invoiceResolver;
        private readonly InvoiceBuilder _invoiceBuilder;
        private readonly InvoiceDirector _invoiceDirector;

        public AuditServiceTests(ApiWebApplicationFactory fixture)
        {
            _fixture = fixture;
            _setup = fixture.CreateSetup();
            _invoiceBuilder = new InvoiceBuilder(new Faker());
            _invoiceDirector = new InvoiceDirector(_invoiceBuilder);
        }

        [Fact]
        public async Task Should_be_able_to_archive_an_audit()
        {
            var client = await _setup.CreateClient(Constants.Account);

            var invoiceDirector = new InvoiceDirector(new InvoiceBuilder(new Faker()));
            var invoice = invoiceDirector.CreateAnonymousSingleProductPaidWithCash(Guid.Parse(client.AccountId));

            var createInvoiceRequest = new CreateInvoiceRequest
            {
                Invoice = invoice.Pack()
            };

            await client.Invoice.CreateAsync(createInvoiceRequest, client.AuthorizedMetadata);

            var createAuditRequest = new CreateAuditRequest
            {
                AccountId = client.AccountId,
                SupplierSystemId = invoice.Supplier.SystemId,
                StartDate = DateTime.UtcNow.AddDays(-1).ToTimestamp(),
                EndDate = DateTime.UtcNow.AddDays(1).ToTimestamp()
            };

            var createAuditReply = await client.Audit.CreateAsync(createAuditRequest, client.AuthorizedMetadata);

            Assert.NotNull(createAuditReply.AuditId);

            var reply = await client.GetAuditReplyAsync(createAuditReply.AuditId);

            Assert.False(string.IsNullOrEmpty(reply.Location));
        }

        /// <summary>
        /// It verifies the number of products after 3 invoices and 2 unique products have been created
        /// </summary>
        [Fact]
        public async Task Should_validate_total_products_added()
        {
            var client = await _setup.CreateClient(Constants.Account);
            var httpClient = _fixture.CreateClient();

            httpClient.DefaultRequestHeaders.Add("Authorization", client.AuthorizedMetadata.GetValue("authorization"));
            _invoiceResolver = new AuditResultsReader(httpClient);

            var product1 = new Vera.Models.Product
            {
                Code = "Beer",
                Description = "an alcoholic drink",
                Type = ProductType.Goods
            };

            var accountId = Guid.Parse(client.AccountId);
            var invoice = _invoiceDirector.CreateAnonymousSingleProductPaidWithCash(accountId, product1);
            var createInvoiceRequest = new CreateInvoiceRequest
            {
                Invoice = invoice.Pack()
            };

            await client.Invoice.CreateAsync(createInvoiceRequest, client.AuthorizedMetadata);

            var product2 = new Vera.Models.Product
            {
                Code = "Bread",
                Description = "staple food",
                Type = ProductType.Goods
            };

            var invoice2 = _invoiceDirector.CreateAnonymousSingleProductPaidWithCash(accountId, product2);
            var createInvoiceRequest2 = new CreateInvoiceRequest
            {
                Invoice = invoice2.Pack()
            };

            await client.Invoice.CreateAsync(createInvoiceRequest2, client.AuthorizedMetadata);
            
            var invoice3 = _invoiceDirector.CreateAnonymousSingleProductPaidWithCash(accountId, product2);
            var createInvoiceRequest3 = new CreateInvoiceRequest
            {
                Invoice = invoice3.Pack()
            };

            await client.Invoice.CreateAsync(createInvoiceRequest3, client.AuthorizedMetadata);


            var getAuditReply = await client.GenerateAuditFile(invoice.Supplier.SystemId);
            var auditProducts = await _invoiceResolver.GetProductsAsync(client.AccountId, getAuditReply.Location);

            Assert.True(auditProducts.Count() == 2);
        }

    }
}