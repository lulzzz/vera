﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Vera.Documents.Visitors;
using Vera.Models;
using Vera.Reports;
using Xunit;
using static Vera.Models.RegisterReport;

namespace Vera.Austria.Tests
{
    public class ReportReceiptGeneratorTests
    {
        [Fact]
        public void Should_generate_report()
        {
            const string registerId = "18.34";
            var employee = new Employee { FirstName = "Employee", LastName = "47" };
            var report = new RegisterReport
            {
                Account = new AccountReport
                {
                    Name = "New Black Austria",
                    TaxRegistrationNumber = "12345"
                },
                SupplierId = Guid.NewGuid(),
                RegisterSystemId = registerId,
                Date = DateTime.Now,
                Discount = new RegisterReport.DiscountReport { Amount = -10m, Count = 1 },
                Payments = new List<PaymentReport>
                {
                    new()
                    {
                        Amount = 2.4079m, Count = 1, PaymentCategory = PaymentCategory.Cash
                    },
                    new()
                    {
                        Amount = 100m, Count = 1, PaymentCategory = PaymentCategory.Debit
                    },
                    new()
                    {
                        Amount = 90m, Count = 1, PaymentCategory = PaymentCategory.Credit
                    }
                },
                PaymentsPerEmployee = new List<EmployeePaymentsReport>
                {
                    new()
                    {
                        Employee = employee,
                        Payment = new PaymentReport{Amount = 2.4079m, Count = 1, PaymentCategory = PaymentCategory.Cash}
                    },
                    new()
                    {
                        Employee = employee,
                        Payment = new PaymentReport{Amount = 100m, Count = 1, PaymentCategory = PaymentCategory.Debit}
                    },
                    new()
                    {
                        Employee = employee,
                        Payment = new PaymentReport{Amount = 90m, Count = 1, PaymentCategory = PaymentCategory.Credit}
                    }
                },
                Products = new List<ProductReport>
                {
                    new() { Amount = 200m, Count = 4, Type = ProductType.Goods}
                },
                Return = new ReturnReport { Count = 1, Amount = -2.41m },
                Taxes = new List<TaxesReport>
                {
                    new() {Amount = 17.78m, TaxesCategory = TaxesCategory.High, TaxRate = 1.21m},
                    new() {Amount = 14.53m, TaxesCategory = TaxesCategory.High, TaxRate = 1.17m}
                },
                Totals = new TotalsReport
                {
                    Cash = 2.41m,
                    Gross = 192.41m,
                    Net = 159.56299145299145299145299145m,
                    Return = -2.41m
                },
                Number = "354",
                Type = RegisterReportType.Current,
                CashDrawerOpenings = 6
            };

            var context = new ReceiptReportContext
            {
                RegisterReport = report,
                Signature = Encoding.ASCII.GetBytes("signature"),
                Header = "TEST",
                Original = false,
                Prints = new List<PrintTrail>
                {
                    new() {},
                }
            };

            var receiptReportGenerator = new ReportReceiptGenerator();
            var node = receiptReportGenerator.Generate(context);

            var sb = new StringBuilder();

            var visitor = new TextThermalVisitor(new StringWriter(sb));
            node.Accept(visitor);

            var result = sb.ToString();

            Assert.NotNull(result);

            Assert.Contains("Payments", result);
            Assert.Contains("Taxes", result);
            Assert.Contains($"KassenID: {registerId}", result);
            Assert.Contains($"TAX NUMBER {report.Account.TaxRegistrationNumber}", result);
            Assert.Contains($"#{report.Number}", result);
        }
    }
}
