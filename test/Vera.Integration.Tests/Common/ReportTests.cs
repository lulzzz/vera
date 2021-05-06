using System;
using System.Linq;
using System.Threading.Tasks;
using Vera.Extensions;
using Vera.Grpc;
using Vera.Host.Mapping;
using Vera.Integration.Tests.Portugal;
using Vera.Invoices;
using Vera.Models;
using Vera.Tests.Scenario;
using Xunit;

namespace Vera.Integration.Tests.Common
{
    public class ReportTests : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly Setup _setup;
        private readonly ApiWebApplicationFactory _fixture;
        private readonly InvoiceTotalsCalculator _calculator;

        public ReportTests(ApiWebApplicationFactory fixture)
        {
            _fixture = fixture;
            _setup = _fixture.CreateSetup();
            _calculator = new InvoiceTotalsCalculator();
        }

        [Fact]
        public async Task Should_generate_register_x_report()
        {
            var client = await _setup.CreateClient(Constants.Account);

            await client.OpenPeriod();
            var openRegisterReply = await client.OpenRegister(100m);

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

            var invoice1 = await CreateInvoice(client, scenario1, openRegisterReply.Id);
            var invoice2 = await CreateInvoice(client, scenario2, openRegisterReply.Id);
            var invoiceReturn = await CreateInvoice(client, returnScenario, openRegisterReply.Id);
            var invoiceDiscount = await CreateInvoice(client, discountScenario, openRegisterReply.Id);

            var request = new GenerateDailyXReportRequest
            {
                RegisterId = openRegisterReply.Id,
                SupplierSystemId = client.SupplierSystemId
            };

            var report = await client.Report.GenerateDailyXReportAsync(request, client.AuthorizedMetadata);

            Assert.NotNull(report);

            var grossTotal = invoice1.Totals.Gross + invoice2.Totals.Gross + invoiceDiscount.Totals.Gross;
            var netTotal = invoice1.Totals.Net + invoice2.Totals.Net + invoiceReturn.Totals.Net + invoiceDiscount.Totals.Net;
            Assert.Equal(3, report.EmployeePayments.Count);
            Assert.Equal(2, report.Payments.Count);
            Assert.Equal(2, report.Taxes.Count);
            Assert.Equal(1, report.Return.Count);
            Assert.Equal(1, report.Discount.Count);
            Assert.Equal(invoiceReturn.Payments.First().Amount, (decimal)report.Return.Amount);
            Assert.Equal(invoiceDiscount.Lines.First().Settlements.First().Amount, (decimal)report.Discount.Amount);
            Assert.Equal(invoice1.Totals.Gross + invoiceDiscount.Totals.Gross, (decimal)report.Totals.Cash);
            Assert.Equal(grossTotal, (decimal)report.Totals.Gross);
            Assert.Equal(netTotal.Round(9), (decimal)report.Totals.Net);
            Assert.Equal(invoiceReturn.Totals.Gross, (decimal)report.Totals.Return);
            Assert.True(!string.IsNullOrEmpty(report.Number));
        }

        internal async Task<Models.Invoice> CreateInvoice(SetupClient client, Scenario scenario, string registerId)
        {
            var result = scenario.Execute();
            var invoice = result.Invoice;
            invoice.RegisterId = registerId;

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
