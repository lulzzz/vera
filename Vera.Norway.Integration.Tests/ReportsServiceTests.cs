using System;
using System.Threading.Tasks;
using Vera.Grpc;
using Vera.Host.Mapping;
using Vera.Integration.Tests;
using Vera.Invoices;
using Vera.Models;
using Vera.Tests.Scenario;
using Xunit;

namespace Vera.Norway.Integration.Tests
{
    public class ReportsServiceTests: IClassFixture<ApiWebApplicationFactory>
    {
        private readonly Setup _setup;
        private readonly InvoiceTotalsCalculator _calculator;

        public ReportsServiceTests(ApiWebApplicationFactory fixture)
        {
            _setup = fixture.CreateSetup();

            _calculator = new InvoiceTotalsCalculator();
        }

        [Fact]
        public async Task Should_be_able_to_render_a_report()
        {
            var client = await _setup.CreateClient(Constants.Account);

            await client.OpenPeriod();
            var registerSystemId = await client.OpenRegister(100m);

            var scenario1 = new SellSingleStaticProductScenario(1.23m, 100m, PaymentCategory.Cash)
            {
                AccountId = Guid.Parse(client.AccountId),
                SupplierSystemId = client.SupplierSystemId
            };

            var scenario2 = new SellSingleStaticProductScenario(1.17m, 100m, PaymentCategory.Credit)
            {
                AccountId = Guid.Parse(client.AccountId),
                SupplierSystemId = client.SupplierSystemId
            };

            var returnScenario = new ReturnInvoiceScenario(TaxesCategory.High, 1.23m, 100m, PaymentCategory.Debit)
            {
                AccountId = Guid.Parse(client.AccountId),
                SupplierSystemId = client.SupplierSystemId
            };

            var discountScenario = new DiscountScenario(1.23m, 100m, 0.10m, PaymentCategory.Cash)
            {
                AccountId = Guid.Parse(client.AccountId),
                SupplierSystemId = client.SupplierSystemId
            };

            var invoice1 = await CreateInvoice(client, scenario1, registerSystemId);
            var invoice2 = await CreateInvoice(client, scenario2, registerSystemId);
            var invoiceReturn = await CreateInvoice(client, returnScenario, registerSystemId);
            var invoiceDiscount = await CreateInvoice(client, discountScenario, registerSystemId);

            var request = new GenerateCurrentReportRequest
            {
                RegisterSystemId = registerSystemId,
                SupplierSystemId = client.SupplierSystemId
            };

            var report = await client.Report.GenerateCurrentReportAsync(request, client.AuthorizedMetadata);

            Assert.NotNull(report);

            var renderReportReply = await client.Report.RenderReportAsync(new RenderReportRequest
            {
                Type = ReportOutputType.Json,
                ReportNumber = report.Number
            }, client.AuthorizedMetadata);

            Assert.NotNull(renderReportReply);

            Assert.Equal(ReportOutputType.Json, renderReportReply.Type);
            Assert.NotNull(renderReportReply.Content);

            var reportContent = renderReportReply.Content.ToStringUtf8();

            Assert.NotNull(reportContent);

            Assert.Contains("X Report ", reportContent);
        }

        internal async Task<Models.Invoice> CreateInvoice(SetupClient client, Scenario scenario, string registerId)
        {
            var result = scenario.Execute();
            var invoice = result.Invoice;
            invoice.RegisterSystemId = registerId;

            var createInvoiceRequest = new Grpc.CreateInvoiceRequest
            {
                Invoice = invoice.Pack()
            };

            await client.Invoice.CreateAsync(createInvoiceRequest, client.AuthorizedMetadata);

            invoice.Totals = _calculator.Calculate(invoice);

            return invoice;
        }
    }
}
