using System;
using System.Collections.Generic;
using System.Text;
using Vera.Documents;
using Vera.Models;
using Xunit;
using Xunit.Abstractions;

namespace Vera.Portugal.Tests
{
    public class ThermalReceiptGeneratorTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public ThermalReceiptGeneratorTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Should_generate_receipt()
        {
            // TODO(kevin): test that receipt contains correct values

            var invoice = new Invoice
            {
                Supplier = new Billable
                {
                    Name = "Lisboa store",
                    RegistrationNumber = "123.123.123",
                    Address = new Address
                    {
                        City = "Lisboa",
                        Number = "180",
                        PostalCode = "1500",
                        Street = "Lalala"
                    },
                },
                Employee = new Billable
                {
                    SystemID = "007",
                    Name = "Kevin"
                },
                TerminalId = "ST01.44",
                Date = DateTime.UtcNow,
                FiscalPeriod = 1,
                FiscalYear = 2020,
                Number = "123",
                OrderReferences = new List<string>{"3001"},
                Lines = new List<InvoiceLine>
                {
                    new InvoiceLine
                    {
                        Gross = 15m,
                        Description = "Shower foam dao",
                        Quantity = 2,
                        Taxes = new Taxes
                        {
                            Rate = 1.21m
                        }
                    }
                },
                Signature = new byte[32],
                Payments = new List<Payment>
                {
                    new Payment
                    {
                        Amount = 10m,
                        Description = "Cash"
                    },
                    new Payment
                    {
                        Amount = 5m,
                        Description = "Debit"
                    }
                },
                Receipts = new List<PinReceipt>
                {
                    new PinReceipt
                    {
                        Lines = new[]
                        {
                            "PIN RECEIPT",
                            "LINES",
                            "GO",
                            "HERE",
                            "MASTERCARD ***123"
                        }
                    }
                }
            };

            var account = new Account
            {
                Name = "Rituals Portugal",
                RegistrationNumber = "123.123.123",
                Address = new Address
                {
                    City = "Lisboa",
                    Number = "180",
                    PostalCode = "1500",
                    Street = "Lalala"
                }
            };

            account.SetConfiguration(new Configuration
            {
                SocialCapital = 258501m
            });

            var context = new ThermalReceiptContextFactory().Create(account, invoice);
            
            var generator = new ThermalReceiptGenerator();
            var node = generator.Generate(context);

            var sb = new StringBuilder();
            
            var visitor = new StringThermalVisitor(sb);
            node.Accept(visitor);

            var result = sb.ToString();

            Assert.Contains("FATURA SIMPLIFICADA", result);
            Assert.DoesNotContain("NOTA DE CRÉDITO", result);

            _testOutputHelper.WriteLine(result);
        }
    }
}