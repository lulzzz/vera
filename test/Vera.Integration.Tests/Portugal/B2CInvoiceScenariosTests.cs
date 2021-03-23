using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vera.Grpc;
using Vera.Grpc.Models;
using Vera.Invoices;
using Vera.Models;
using Vera.Portugal.Models;
using Vera.Tests.Scenario;
using Xunit;

namespace Vera.Integration.Tests.Portugal
{
    public class B2CInvoiceScenariosTests : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly Setup _setup;
        private readonly ApiWebApplicationFactory _fixture;
        private AuditResultsStore _auditResultsStore;

        public B2CInvoiceScenariosTests(ApiWebApplicationFactory fixture)
        {
            _fixture = fixture;
            _setup = fixture.CreateSetup();
        }
        
        [Fact]
        public async Task Test_B2C_Scenarios()
        {
            var client = await _setup.CreateClient(Constants.Account);
            
            var httpClient = _fixture.CreateClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", client.AuthorizedMetadata.GetValue("authorization"));
            
            _auditResultsStore = new AuditResultsStore(httpClient);

            var allTaxRates = new Dictionary<TaxesCategory, decimal>()
            {
                {TaxesCategory.High, 1.23m},
                {TaxesCategory.Intermediate, 1.16m},
                {TaxesCategory.Low, 1.06m},
                {TaxesCategory.Zero, 1m},
            };
            
            // Scenarios from top to bottom
            // https://www.notion.so/Pre-audit-Portugal-79e136a1932847818b7d9f44c09db0e5
            var scenarios = new Test[]
            {
                new()
                {
                    Scenario = new SellSingleStaticProductScenario(1.23m, 123m, PaymentCategory.Cash),
                    Expected = new()
                    {
                        Type = InvoiceType.FR,
                        Net = 100,
                        Gross = 123
                    }
                },
                new()
                {
                    Scenario = new SellSingleStaticProductScenario(1.13m, 113m, PaymentCategory.Credit),
                    Expected = new()
                    {
                        Type = InvoiceType.FR,
                        Net = 100,
                        Gross = 113
                    }
                },
                new()
                {
                    Scenario = new SellSingleStaticProductScenario(1m, 100m, PaymentCategory.Debit),
                    Expected = new()
                    {
                        Type = InvoiceType.FR,
                        Net = 100,
                        Gross = 100
                    }
                },
                new()
                {
                    Scenario = new MultipleTaxRateScenario(allTaxRates, 100m),
                    Expected = new()
                    {
                        Type = InvoiceType.FR,
                        Net = 67.88m,
                        Gross = 100
                    }
                },
                new()
                {
                    Scenario = new MultiplePaymentScenario(1.23m, new[]
                    {
                        (PaymentCategory.Debit, 50m),
                        (PaymentCategory.Cash,  50m)
                    }),
                    Expected = new()
                    {
                        Type = InvoiceType.FR,
                        Net = 81.3m,
                        Gross = 100m
                    }
                },
                new()
                {
                    Scenario = new MultiplePaymentScenario(1.23m, new[]
                    {
                        (PaymentCategory.Debit, 300m),
                        (PaymentCategory.Voucher, 200m)
                    }),
                    Expected = new()
                    {
                        Type = InvoiceType.FR,
                        Net = 406.5m,
                        Gross = 500m
                    }
                },
                new()
                {
                    Scenario = new ReturnInvoiceScenario(TaxesCategory.High, 1.23m, 100m, PaymentCategory.Cash),
                    Expected = new()
                    {
                        Type = InvoiceType.NC,
                        Net = 81.3m,
                        Gross = 100m
                    }
                },
                new()
                {
                    Scenario = new ReturnInvoiceScenario(TaxesCategory.Intermediate, 1.13m, 100m, PaymentCategory.Credit),
                    Expected = new()
                    {
                        Type = InvoiceType.NC,
                        Net = 88.5m,
                        Gross = 100m
                    }
                },
                new()
                {
                    Scenario = new ReturnInvoiceScenario(TaxesCategory.Low,1.06m, 100m, PaymentCategory.Cash),
                    Expected = new()
                    {
                        Type = InvoiceType.NC,
                        Net = 94.34m,
                        Gross = 100m
                    }
                },
                new()
                {
                    Scenario = new ReturnInvoiceScenario(allTaxRates, 100m, PaymentCategory.Credit),
                    Expected = new()
                    {
                        Type = InvoiceType.NC,
                        Net = 100m,
                        Gross = 100m
                    }
                },
                new()
                {
                    Scenario = new DiscountScenario(1.23m, 100m, 0.10m, PaymentCategory.Cash),
                    Expected = new()
                    {
                        Type = InvoiceType.FR,
                        Net = 73.17m,
                        Gross = 90m
                    }
                },
                new()
                {
                    Scenario = new DiscountScenario(1.13m, 100m, 0.10m, PaymentCategory.Debit),
                    Expected = new()
                    {
                        Type = InvoiceType.FR,
                        Net = 79.75m,
                        Gross = 90m
                    }
                },
                new()
                {
                    Scenario = new DiscountScenario(1m, 100m, 0.10m, PaymentCategory.Credit),
                    Expected = new()
                    {
                        Type = InvoiceType.FR,
                        Net = 90m,
                        Gross = 90m
                    }
                },
                new()
                {
                    Scenario = new MixedTaxRatesWithDiscountScenario(allTaxRates, 100m, 100m * 0.1m),
                    Expected = new()
                    {
                        Type = InvoiceType.FR,
                        Net = 59.51m,
                        Gross = 90m
                    }
                },
                new()
                {
                    Scenario = new MixedTaxRatesWithDiscountScenario(allTaxRates, 100m, 100m * 0.088m),
                    Expected = new()
                    {
                        Type = InvoiceType.FR,
                        Net = 60.77m,
                        Gross = 91.91m
                    }
                },
                new()
                {
                    Scenario = new MixedTaxRatesWithDiscountScenario(
                        new Dictionary<TaxesCategory, decimal>
                        {
                            {TaxesCategory.High, 1.23m},
                            {TaxesCategory.Zero, 1m}
                        }, 
                        100m, 
                        100m * 0.088m
                    ),
                    Expected = new()
                    {
                        Type = InvoiceType.FR,
                        Net = 74.72m,
                        Gross = 91.91m
                    }
                }
            };

            await client.OpenPeriod();
            
            foreach (var test in scenarios)
            {
                var scenario = test.Scenario;
                var expected = test.Expected;
                
                scenario.AccountId = Guid.Parse(client.AccountId);
                scenario.SupplierSystemId = client.SupplierSystemId;

                var result = scenario.Execute();
                
                var createInvoiceRequest = new CreateInvoiceRequest
                {
                    Invoice = result.Invoice.Pack()
                };
        
                var reply = await client.Invoice.CreateAsync(createInvoiceRequest, client.AuthorizedMetadata);

                Assert.Contains(expected.Type.ToString(), reply.Number);

                _auditResultsStore.AddExpectedEntry(new InvoiceResult
                {
                    Invoice = result.Invoice,
                    InvoiceNumber = reply.Number,
                    InvoiceType = expected.Type,
                    GrossTotal = expected.Gross,
                    NetTotal = expected.Net
                });
            }

            var getAuditReply = await client.GenerateAuditFile(client.SupplierSystemId);
            
            await _auditResultsStore.LoadInvoicesFromAuditAsync(client.AccountId, getAuditReply.Location);

            var calculator = new InvoiceTotalsCalculator();
            
            foreach (var expected in _auditResultsStore.ExpectedResults)
            {
                var got = _auditResultsStore.GetAuditEntry(expected.InvoiceNumber);

                Assert.NotNull(got);

                var expectedInvoice = expected.Invoice;
                
                Assert.Equal(expected.InvoiceType, got.InvoiceType);
                
                var totals = calculator.Calculate(expectedInvoice);
                
                Assert.Equal(Round(totals.Gross, 2), got.GrossTotal);
                Assert.Equal(Round(totals.Net, 2), got.NetTotal);
            }
        }
        
        private static decimal Round(decimal d, int decimals) => Math.Round(Math.Abs(d), decimals);
        
        public class Test
        {
            public Scenario Scenario { get; set; }
            public ExpectedResult Expected { get; set; }

            public class ExpectedResult
            {
                public InvoiceType Type { get; set; }
                public decimal Gross { get; set; }
                public decimal Net { get; set; }
            }
        }
    }
}
