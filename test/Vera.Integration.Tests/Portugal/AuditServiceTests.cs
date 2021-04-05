using System;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Vera.Grpc;
using Vera.Grpc.Models;
using Vera.Host.Security;
using Vera.Models;
using Vera.Tests.Scenario;
using Vera.Tests.Shared;
using Xunit;

namespace Vera.Integration.Tests.Portugal
{
    public class AuditServiceTests : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly Setup _setup;
        private readonly ApiWebApplicationFactory _fixture;

        public AuditServiceTests(ApiWebApplicationFactory fixture)
        {
            _fixture = fixture;
            _setup = fixture.CreateSetup();
        }

        [Fact]
        public async Task Should_be_able_to_archive_an_audit()
        {
            var client = await _setup.CreateClient(Constants.Account);

            var scenario = new SellSingleStaticProductScenario(1.23m, 100m, PaymentCategory.Cash)
            {
                AccountId = Guid.Parse(client.AccountId),
                SupplierSystemId = client.SupplierSystemId
            };
            
            var result = scenario.Execute();

            await client.OpenPeriod();

            var openRegisterReply = await client.OpenRegister(100m);
            var invoice = result.Invoice;
            invoice.RegisterId = openRegisterReply.Id;

            var createInvoiceRequest = new CreateInvoiceRequest
            {
                Invoice = invoice.Pack()
            };

            await client.Invoice.CreateAsync(createInvoiceRequest, client.AuthorizedMetadata);

            var createAuditRequest = new CreateAuditRequest
            {
                AccountId = client.AccountId,
                SupplierSystemId = result.Invoice.Supplier.SystemId,
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

            httpClient.DefaultRequestHeaders.Add("Authorization", client.AuthorizedMetadata.GetValue(MetadataKeys.Authorization));

            var invoiceResolver = new AuditResultsStore(httpClient);

            var product1 = ProductFactory.CreateRandomProduct();
            var product2 = ProductFactory.CreateRandomProduct();

            var products = new[]
            {
                product1, product2, product2
            };

            await client.OpenPeriod();

            var openRegisterReply = await client.OpenRegister(100m);

            foreach (var product in products)
            {
                var scenario = new SellProductScenario(product)
                {
                    AccountId = Guid.Parse(client.AccountId),
                    SupplierSystemId = client.SupplierSystemId
                };

                var result = scenario.Execute();

                var invoice = result.Invoice;
                invoice.RegisterId = openRegisterReply.Id;

                await client.Invoice.CreateAsync(new CreateInvoiceRequest
                {
                    Invoice = invoice.Pack()
                }, client.AuthorizedMetadata);
            }

            var getAuditReply = await client.GenerateAuditFile();
            var auditProducts = await invoiceResolver.LoadProductsFromAuditAsync(client.AccountId, getAuditReply.Location);

            Assert.Equal(2, auditProducts.Count());
        }

    }
}