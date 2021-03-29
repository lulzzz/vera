using System;
using System.Linq;
using System.Threading.Tasks;
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

        public AuditServiceTests(ApiWebApplicationFactory fixture)
        {
            _fixture = fixture;
            _setup = fixture.CreateSetup();
        }

        [Fact]
        public async Task Should_be_able_to_archive_an_audit()
        {
            var client = await _setup.CreateClient(Constants.Account);

            var builder = new InvoiceBuilder();
            var director = new InvoiceDirector(builder, Guid.Parse(client.AccountId));
            director.ConstructAnonymousWithSingleProductPaidWithCash();

            var invoice = builder.Result;

            var dataProvider = new SampleDataProvier(client);
            var supplier = await dataProvider.CreateSupplier(invoice.Supplier.SystemId);
    
            await dataProvider.CreateOpenPeriod(supplier.SystemId);
            
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
            var dataProvider = new SampleDataProvier(client);
            var supplier = await dataProvider.CreateSupplier();

            var builder = new InvoiceBuilder();
            var director = new InvoiceDirector(builder, Guid.Parse(client.AccountId), supplier.SystemId);
            director.ConstructAnonymousWithSingleProductPaidWithCash(product1);

            var invoice = builder.Result;
            
            await dataProvider.CreateOpenPeriod(supplier.SystemId);

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
            
            director.ConstructAnonymousWithSingleProductPaidWithCash(product2);

            var invoice2 = builder.Result;
            invoice2.PeriodId = invoice.PeriodId;

            var createInvoiceRequest2 = new CreateInvoiceRequest
            {
                Invoice = invoice2.Pack()
            };

            await client.Invoice.CreateAsync(createInvoiceRequest2, client.AuthorizedMetadata);
            
            director.ConstructAnonymousWithSingleProductPaidWithCash(product2);

            var invoice3 = builder.Result;
            invoice3.PeriodId = invoice.PeriodId;

            var createInvoiceRequest3 = new CreateInvoiceRequest
            {
                Invoice = invoice3.Pack()
            };

            await client.Invoice.CreateAsync(createInvoiceRequest3, client.AuthorizedMetadata);


            var getAuditReply = await client.GenerateAuditFile(supplier.SystemId);
            var auditProducts = await _invoiceResolver.GetProductsAsync(client.AccountId, getAuditReply.Location);

            Assert.Equal(2, auditProducts.Count());
        }

    }
}