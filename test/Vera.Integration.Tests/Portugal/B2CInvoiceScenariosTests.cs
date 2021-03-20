// using Bogus;
// using Google.Protobuf.WellKnownTypes;
// using System;
// using System.Linq;
// using System.Threading.Tasks;
// using Vera.Grpc;
// using Vera.Grpc.Models;
// using Vera.Models;
// using Vera.Tests.Shared;
// using Xunit;
//
// namespace Vera.Integration.Tests.Portugal
// {
//     public class B2CInvoiceScenariosTests : IClassFixture<ApiWebApplicationFactory>
//     {
//         private readonly Setup _setup;
//         private readonly InvoiceBuilder _invoiceBuilder;
//         private readonly InvoiceDirector _invoiceDirector;
//         private readonly ApiWebApplicationFactory _fixture;
//         private AuditResultsReader _invoiceResolver;
//
//         public B2CInvoiceScenariosTests(ApiWebApplicationFactory fixture)
//         {
//             _fixture = fixture;
//             _setup = fixture.CreateSetup();
//             _invoiceBuilder = new InvoiceBuilder(new Faker());
//             _invoiceDirector = new InvoiceDirector(_invoiceBuilder);
//         }
//
//         [Fact]
//         public async Task Test_B2C_Scenarios()
//         {
//             var client = await _setup.CreateClient(Constants.Account);
//             var httpClient = _fixture.CreateClient();
//
//             httpClient.DefaultRequestHeaders.Add("Authorization", client.AuthorizedMetadata.GetValue("authorization"));
//             _invoiceResolver = new AuditResultsReader(httpClient);
//
//             // TODO(kevin): allow passing supplier to the invoice builder/director
//             
//             // Scenarios 1-3
//             var invoice = await CreateSimpleScenario(1.23m, PaymentCategory.Cash);
//             // await CreateSimpleScenario(1.13m, PaymentCategory.Credit);
//             // await CreateSimpleScenario(1m, PaymentCategory.Debit);
//             await CompletePurchaseWithMultipleVATs();
//             // await CreateInvoiceWithMultiplePayments();
//             // await CreateInvoiceWithMultiplePaymentsVoucher();
//             // await CreateInvoiceWithDiscounts(1.23m, 0.10m, PaymentCategory.Cash);
//             // await CreateInvoiceWithDiscounts(1.13m, 0.10m, PaymentCategory.Debit);
//             // await CreateInvoiceWithDiscounts(1m, 0.10m, PaymentCategory.Credit);
//             // await CompletePurchaseWithMultipleVATsAndDiscount();
//             // await CompletePurchaseWithMultipleVATsAndLineDiscount();
//             // await CompletePurchaseWithMultipleVATsAndDiscount2();
//             // await CreateReturnInvoice(1.23m, PaymentCategory.Cash);
//             // await CreateReturnInvoice(1.13m, PaymentCategory.Cash);
//             // await CreateReturnInvoice(1.06m, PaymentCategory.Cash);
//
//             var getAuditReply = await client.GenerateAuditFile(invoice.Supplier.SystemId);
//
//             await _invoiceResolver.LoadInvoiceResultsAsync(client.AccountId, getAuditReply.Location);
//
//             foreach (var expectedInvoice in _invoiceResolver.ExpectedResults)
//             {
//                 var actualResult = _invoiceResolver.GetActualResult(expectedInvoice.InvoiceNumber);
//
//                 Assert.NotNull(actualResult);
//
//                 Assert.Equal(expectedInvoice.InvoiceNumber, actualResult.InvoiceNumber);
//                 Assert.Equal(expectedInvoice.ATCUD, actualResult.ATCUD);
//                 Assert.Equal(expectedInvoice.InvoiceType, actualResult.InvoiceType);
//                 Assert.Equal(Round(expectedInvoice.GrossTotal, 2), actualResult.GrossTotal);
//                 Assert.Equal(Round(expectedInvoice.NetTotal, 2), actualResult.NetTotal);
//                 Assert.Equal(expectedInvoice.InvoiceLinesCount, actualResult.InvoiceLinesCount);
//             }
//         }
//
//         private static decimal Round(decimal d, int decimals) => Math.Round(Math.Abs(d), decimals);
//
//         /// <summary>
//         /// B2C Scenarios 1-3
//         /// </summary>
//         internal async Task<Models.Invoice> CreateSimpleScenario(decimal taxRate, PaymentCategory paymentMethod)
//         {
//             const decimal unitPrice = 100m;
//             const int quantity = 1;
//
//             var client = await _setup.CreateClient(Constants.Account);
//             var accountId = Guid.Parse(client.AccountId);
//             var invoice = _invoiceDirector.CreateInvoice(accountId, unitPrice, quantity, taxRate, paymentMethod);
//
//             var createInvoiceRequest = new CreateInvoiceRequest
//             {
//                 Invoice = invoice.Pack()
//             };
//
//             var createInvoiceReply = await client.Invoice.CreateAsync(createInvoiceRequest, client.AuthorizedMetadata);
//
//             Assert.Equal($"itFR {invoice.Supplier.SystemId}/{createInvoiceReply.Sequence}", createInvoiceReply.Number);
//             Assert.True(createInvoiceReply.Sequence > 0);
//
//             _invoiceResolver.Add(new InvoiceResult
//             {
//                 InvoiceNumber = createInvoiceReply.Number,
//                 ATCUD = "0",
//                 InvoiceType = Vera.Portugal.Models.InvoiceType.FR,
//                 NetTotal = unitPrice,
//                 GrossTotal = unitPrice * taxRate,
//                 InvoiceLinesCount = 1,
//                 ProductsCount = 1
//                 // TODO assert payment type
//             });
//
//             return invoice;
//         }
//
//         /// <summary>
//         /// B2C Scenarios 4
//         /// </summary>
//         internal async Task CompletePurchaseWithMultipleVATs()
//         {
//             const decimal unitPrice = 25m;
//             var taxRates = new decimal[] { 1.23m, 1.13m, 1.06m, 1m };
//
//             var client = await _setup.CreateClient(Constants.Account);
//             var invoice = _invoiceDirector.CreatePurchaseMultipleProducts(Guid.Parse(client.AccountId), unitPrice, PaymentCategory.Cash, taxRates);
//
//             var createInvoiceRequest = new CreateInvoiceRequest
//             {
//                 Invoice = invoice.Pack()
//             };
//
//             var createInvoiceReply = await client.Invoice.CreateAsync(createInvoiceRequest, client.AuthorizedMetadata);
//
//             Assert.NotNull(createInvoiceReply.Number);
//
//             _invoiceResolver.Add(new InvoiceResult
//             {
//                 InvoiceNumber = createInvoiceReply.Number,
//                 ATCUD = "0",
//                 InvoiceType = Vera.Portugal.Models.InvoiceType.FR,
//                 NetTotal = taxRates.Aggregate(0m, (total, rate) => total + unitPrice / rate),
//                 GrossTotal = unitPrice * taxRates.Length,
//                 InvoiceLinesCount = taxRates.Length,
//                 ProductsCount = taxRates.Length
//                 // TODO assert payment type
//             }); ; ;
//         }
//
//         /// <summary>
//         /// B2C Scenario 5
//         /// </summary>
//         internal async Task CreateInvoiceWithMultiplePayments()
//         {
//             const decimal amount = 100m;
//             const decimal taxRate = 1.23m;
//
//             var client = await _setup.CreateClient(Constants.Account);
//
//
//             var builder = new InvoiceBuilder(new Faker());
//             var director = new InvoiceDirector(builder, Guid.Parse(client.AccountId), "1");
//             director.ConstructAnonymousWithoutLines();
//
//             builder.Build();
//
//             _invoiceDirector.ConstructAnonymousWithoutLines(Guid.Parse(client.AccountId));
//
//             var invoice = _invoiceBuilder
//                 .WithPayment(50m, PaymentCategory.Debit)
//                 .WithPayment(50m, PaymentCategory.Cash)
//                 .WithAmount(amount, taxRate)
//                 .Build();
//
//             var createInvoiceRequest = new CreateInvoiceRequest
//             {
//                 Invoice = invoice.Pack()
//             };
//
//             var createInvoiceReply = await client.Invoice.CreateAsync(createInvoiceRequest, client.AuthorizedMetadata);
//
//             Assert.Contains("FR", createInvoiceReply.Number);
//             Assert.NotNull(createInvoiceReply.Number);
//
//             _invoiceResolver.Add(new InvoiceResult
//             {
//                 InvoiceNumber = createInvoiceReply.Number,
//                 ATCUD = "0",
//                 InvoiceType = Vera.Portugal.Models.InvoiceType.FR,
//                 NetTotal = amount / taxRate,
//                 GrossTotal = amount,
//                 InvoiceLinesCount = 1
//                 // TODO assert payment type
//             });
//         }
//
//         /// <summary>
//         /// B2C Scenario 6
//         /// TODO Scenario 7 - multiple purpose voucher
//         /// </summary>
//         internal async Task CreateInvoiceWithMultiplePaymentsVoucher()
//         {
//             const decimal amount = 500m;
//             const decimal taxRate = 1.23m;
//
//             var client = await _setup.CreateClient(Constants.Account);
//
//             _invoiceDirector.ConstructAnonymousWithoutLines(Guid.Parse(client.AccountId));
//
//             var invoice = _invoiceBuilder
//                 .WithPayment(300m, PaymentCategory.Debit)
//                 .WithPayment(200m, PaymentCategory.Voucher)
//                 .WithAmount(amount, taxRate)
//                 .Build();
//
//             var createInvoiceRequest = new CreateInvoiceRequest
//             {
//                 Invoice = invoice.Pack()
//             };
//
//             var createInvoiceReply = await client.Invoice.CreateAsync(createInvoiceRequest, client.AuthorizedMetadata);
//
//             Assert.Contains("FR", createInvoiceReply.Number);
//             Assert.NotNull(createInvoiceReply.Number);
//
//             _invoiceResolver.Add(new InvoiceResult
//             {
//                 InvoiceNumber = createInvoiceReply.Number,
//                 ATCUD = "0",
//                 InvoiceType = Vera.Portugal.Models.InvoiceType.FR,
//                 NetTotal = amount / taxRate,
//                 GrossTotal = amount,
//                 InvoiceLinesCount = 1
//                 // TODO assert payment type
//             });
//         }
//
//         /// <summary>
//         /// B2C Scenario 8
//         /// </summary>
//         internal async Task CreateReturnInvoice(decimal taxRate, PaymentCategory paymentCategory)
//         {
//             // create invoice
//             const decimal unitPrice = 100m;
//             const int quantity = 1;
//             const decimal amount = unitPrice * quantity;
//
//             var client = await _setup.CreateClient(Constants.Account);
//             var accountId = Guid.Parse(client.AccountId);
//             var invoice = _invoiceDirector.CreateInvoice(accountId, unitPrice, quantity, taxRate, paymentCategory);
//
//             var createInvoiceRequest = new CreateInvoiceRequest
//             {
//                 Invoice = invoice.Pack()
//             };
//
//             var createInvoiceReply = await client.Invoice.CreateAsync(createInvoiceRequest, client.AuthorizedMetadata);
//
//             Assert.Equal($"itFR {invoice.Supplier.SystemId}/{createInvoiceReply.Sequence}", createInvoiceReply.Number);
//             Assert.True(createInvoiceReply.Sequence > 0);
//
//             // create return invoice
//             invoice.Number = createInvoiceReply.Number;
//             var returnInvoice = _invoiceDirector.CreateReturnInvoice(accountId, unitPrice, (-1) * quantity, taxRate, paymentCategory, invoice);
//
//             var createReturnInvoiceRequest = new CreateInvoiceRequest
//             {
//                 Invoice = returnInvoice.Pack()
//             };
//
//             var createReturnInvoiceReply = await client.Invoice.CreateAsync(createReturnInvoiceRequest, client.AuthorizedMetadata);
//
//
//             _invoiceResolver.Add(new InvoiceResult
//             {
//                 InvoiceNumber = createReturnInvoiceReply.Number,
//                 ATCUD = "0",
//                 InvoiceType = Vera.Portugal.Models.InvoiceType.NC,
//                 NetTotal = -amount / taxRate,
//                 GrossTotal = -amount,
//                 InvoiceLinesCount = 1
//                 // TODO assert payment type
//             });
//
//         }
//
//         /// <summary>
//         /// B2C Scenario 12-14
//         /// </summary>
//         internal async Task CreateInvoiceWithDiscounts(decimal taxRate, decimal discountRate, PaymentCategory paymentMethod)
//         {
//             const decimal unitPrice = 100m;
//             const int quantity = 1;
//             const decimal amountInTax = unitPrice * quantity;
//             var amount = amountInTax / taxRate;
//
//             var client = await _setup.CreateClient(Constants.Account);
//             var invoice = _invoiceDirector.CreatePurchaseWithSettlement(Guid.Parse(client.AccountId), unitPrice, quantity, taxRate, paymentMethod, discountRate);
//
//             var createInvoiceRequest = new CreateInvoiceRequest
//             {
//                 Invoice = invoice.Pack()
//             };
//
//             var createInvoiceReply = await client.Invoice.CreateAsync(createInvoiceRequest, client.AuthorizedMetadata);
//
//             Assert.Contains("FR", createInvoiceReply.Number);
//             Assert.NotNull(createInvoiceReply.Number);
//
//             _invoiceResolver.Add(new InvoiceResult
//             {
//                 InvoiceNumber = createInvoiceReply.Number,
//                 ATCUD = "0",
//                 InvoiceType = Vera.Portugal.Models.InvoiceType.FR,
//                 NetTotal = amount - discountRate * amountInTax,
//                 GrossTotal = amountInTax,
//                 InvoiceLinesCount = 1
//                 // TODO assert payment type
//             });
//         }
//
//         /// <summary>
//         /// B2C Scenarios 15
//         /// </summary>
//         internal async Task CompletePurchaseWithMultipleVATsAndDiscount()
//         {
//             const decimal lineAmount = 25m;
//             const decimal discountRate = 0.10m;
//             var taxRates = new decimal[] { 1.23m, 1.13m, 1.06m, 1m };
//             var amountInTax = lineAmount * taxRates.Length;
//             var amount = taxRates.Aggregate(0m, (total, rate) => total + lineAmount / rate);
//             var discount = (-1) * discountRate * amountInTax;
//
//             var client = await _setup.CreateClient(Constants.Account);
//             var invoice = _invoiceDirector.CreatePurchaseMultipleProducts(Guid.Parse(client.AccountId), lineAmount, PaymentCategory.Debit, taxRates, discount: discount);
//
//             var createInvoiceRequest = new CreateInvoiceRequest
//             {
//                 Invoice = invoice.Pack()
//             };
//
//             var createInvoiceReply = await client.Invoice.CreateAsync(createInvoiceRequest, client.AuthorizedMetadata);
//
//             Assert.NotNull(createInvoiceReply.Number);
//
//             _invoiceResolver.Add(new InvoiceResult
//             {
//                 InvoiceNumber = createInvoiceReply.Number,
//                 ATCUD = "0",
//                 InvoiceType = Vera.Portugal.Models.InvoiceType.FR,
//                 NetTotal = amount, // TOTO add invoice discount if it must be included
//                 GrossTotal = amountInTax,
//                 InvoiceLinesCount = taxRates.Length
//                 // TODO assert payment type
//             });
//         }
//
//         /// <summary>
//         /// B2C Scenarios 16
//         /// </summary>
//         internal async Task CompletePurchaseWithMultipleVATsAndLineDiscount()
//         {
//             const decimal lineAmount = 25m;
//             const decimal lineDiscount = 0.08m;
//             var taxRates = new decimal[] { 1.23m, 1.13m, 1.06m, 1m };
//             var grossAmount = lineAmount * taxRates.Length;
//             var netAmount = taxRates.Aggregate(0m, (total, rate) => total + lineAmount / rate - lineAmount * lineDiscount);
//
//             var client = await _setup.CreateClient(Constants.Account);
//             var invoice = _invoiceDirector.CreatePurchaseMultipleProducts(Guid.Parse(client.AccountId), lineAmount, PaymentCategory.Debit, taxRates, lineDiscount: lineDiscount);
//
//             var createInvoiceRequest = new CreateInvoiceRequest
//             {
//                 Invoice = invoice.Pack()
//             };
//
//             var createInvoiceReply = await client.Invoice.CreateAsync(createInvoiceRequest, client.AuthorizedMetadata);
//
//             Assert.NotNull(createInvoiceReply.Number);
//
//             _invoiceResolver.Add(new InvoiceResult
//             {
//                 InvoiceNumber = createInvoiceReply.Number,
//                 ATCUD = "0",
//                 InvoiceType = Vera.Portugal.Models.InvoiceType.FR,
//                 NetTotal = netAmount,
//                 GrossTotal = grossAmount,
//                 InvoiceLinesCount = taxRates.Length
//                 // TODO assert payment type
//             });
//         }
//
//         // <summary>
//         /// B2C Scenarios 17
//         /// </summary>
//         internal async Task CompletePurchaseWithMultipleVATsAndDiscount2()
//         {
//             const decimal lineAmount = 100m;
//             const decimal discountRate = 0.10m;
//             var taxRates = new decimal[] { 1.23m, 1.23m, 1.23m, 1m };
//             var amountInTax = lineAmount * taxRates.Length;
//             var amount = taxRates.Aggregate(0m, (total, rate) => total + lineAmount / rate);
//             var discount = (-1) * discountRate * amountInTax;
//
//             var client = await _setup.CreateClient(Constants.Account);
//             var invoice = _invoiceDirector.CreatePurchaseMultipleProducts(Guid.Parse(client.AccountId), lineAmount, PaymentCategory.Debit, taxRates, discount: discount);
//
//             var createInvoiceRequest = new CreateInvoiceRequest
//             {
//                 Invoice = invoice.Pack()
//             };
//
//             var createInvoiceReply = await client.Invoice.CreateAsync(createInvoiceRequest, client.AuthorizedMetadata);
//
//             Assert.NotNull(createInvoiceReply.Number);
//
//             _invoiceResolver.Add(new InvoiceResult
//             {
//                 InvoiceNumber = createInvoiceReply.Number,
//                 ATCUD = "0",
//                 InvoiceType = Vera.Portugal.Models.InvoiceType.FR,
//                 NetTotal = amount,
//                 GrossTotal = amountInTax,
//                 InvoiceLinesCount = taxRates.Length
//                 // TODO assert payment type
//             });
//         }
//     }
// }
