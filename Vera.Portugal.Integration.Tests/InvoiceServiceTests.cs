using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vera.Dependencies;
using Vera.Grpc;
using Vera.Host.Mapping;
using Vera.Host.Security;
using Vera.Integration.Tests;
using Vera.Models;
using Vera.Tests.Scenario;
using Vera.Tests.Shared;
using Xunit;

namespace Vera.Portugal.Integration.Tests
{
    public class InvoiceServiceTests : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly Setup _setup;
        private readonly ApiWebApplicationFactory _fixture;

        public InvoiceServiceTests(ApiWebApplicationFactory fixture)
        {
            _setup = fixture.CreateSetup();
            _fixture = fixture;
        }

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

            Assert.Equal($"itFR {invoice.Supplier.SystemId}/{createInvoiceReply.Sequence}", createInvoiceReply.Number);
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
        public async Task Should_be_able_to_get_validation_results()
        {
            var client = await _setup.CreateClient(Constants.Account);

            var scenario = new SellProductScenario(ProductFactory.CreateRandomProduct())
            {
                AccountId = Guid.Parse(client.AccountId),
                SupplierSystemId = client.SupplierSystemId
            };

            var result = scenario.Execute();
            var invoice = result.Invoice;

            invoice.Lines.Add(new Vera.Models.InvoiceLine
            {
                Description = "trigger mixed quantities",
                Product = ProductFactory.CreateRandomProduct(),
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

        [Fact]
        public async Task Should_create_invoice_with_working_document()
        {
            var dateProvider = new RealLifeDateProvider();
            var startDate = dateProvider.Now;
            var client = await _setup.CreateClient(Constants.Account);
            var httpClient = _fixture.CreateClient();

            httpClient.DefaultRequestHeaders.Add("Authorization", client.AuthorizedMetadata.GetValue(MetadataKeys.Authorization));

            var auditResultsStore = new AuditResultsStore(httpClient);

            var product = new Vera.Models.Product
            {
                Code = "Gift",
                Type = ProductType.GiftCard,
                SystemId = "ean",
                Description = "desc"
            };

            await client.OpenPeriod();
            var registerSystemId = await client.OpenRegister(100m);

            var builder = new InvoiceBuilder();
            var invoice = builder
                .Reset()
                .WithAccount(Guid.Parse(client.AccountId))
                .WithRegister(registerSystemId)
                .WithEmployee()
                .WithCustomer()
                .WithSupplier(client.SupplierSystemId)
                .WithProductLine(1, 1.99m, 1.23m, TaxesCategory.High, product)
                .WithPayment(PaymentCategory.Cash)
                .WithSignature(new Signature
                {
                    Input = "test",
                    Output = Encoding.ASCII.GetBytes("test"),
                    Version = 1
                })
                .Build();

            var createInvoiceRequest = new CreateInvoiceRequest
            {
                Invoice = invoice.Pack()
            };

            await client.Invoice.CreateAsync(createInvoiceRequest, client.AuthorizedMetadata);

            var invoice2 = builder
               .Reset()
               .WithAccount(Guid.Parse(client.AccountId))
               .WithRegister("1.1")
               .WithEmployee()
               .WithCustomer()
               .WithSupplier(client.SupplierSystemId)
               .WithProductLine(1, 1.99m, 1.23m, TaxesCategory.High, ProductFactory.CreateRandomProduct())
               .WithPayment(PaymentCategory.Cash)
               .WithSignature(new Signature
               {
                   Input = "test",
                   Output = Encoding.ASCII.GetBytes("test"),
                   Version = 1
               })
               .Build();

            invoice2.RegisterSystemId = registerSystemId;

            var createInvoiceRequest2 = new CreateInvoiceRequest
            {
                Invoice = invoice2.Pack()
            };

            var createInvoiceReply2 = await client.Invoice.CreateAsync(createInvoiceRequest2, client.AuthorizedMetadata);


            // should exists 2 invoices and 1 working document
            var getAuditReply = await client.GenerateAuditFile(startDate);
            var (invoices, workingDocuments) = await auditResultsStore.LoadInvoicesAndWorkingDocumentsFromAuditAsync(client.AccountId, getAuditReply.Location);

            Assert.True(invoices.Count() == 2);
            Assert.True(workingDocuments.Count() == 1);
            Assert.Contains(invoices, i => i.Number == createInvoiceReply2.Number);

            var wdGiftCardLine = workingDocuments.First().Lines.First();
            var invoiceGiftCardLine = invoice.Lines.First();

            Assert.True(wdGiftCardLine.Gross == invoiceGiftCardLine.Gross);
            Assert.True(wdGiftCardLine.Product.Code == invoiceGiftCardLine.Product.Code);
        }
    }
}
