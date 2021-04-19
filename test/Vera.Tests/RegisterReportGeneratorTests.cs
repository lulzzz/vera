using Google.Protobuf.Collections;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vera.Audits;
using Vera.Bootstrap;
using Vera.Dependencies;
using Vera.Dependencies.Handlers;
using Vera.Invoices;
using Vera.Models;
using Vera.Reports;
using Vera.Stores;
using Vera.Tests.Shared;
using Xunit;

namespace Vera.Tests
{
    public class RegisterReportGeneratorTests
    {
        [Fact]
        public async Task Should_generate_empty_report()
        {
            var dateProvider = new Mock<RealLifeDateProvider>();
            var invoiceStore = new Mock<IInvoiceStore>();
            var supplierStore = new Mock<ISupplierStore>();
            var accountStore = new Mock<IAccountStore>();

            invoiceStore.Setup(i => i.List(It.IsAny<AuditCriteria>()))
                .ReturnsAsync(new List<Invoice>());

            supplierStore.Setup(s => s.Get(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(new Supplier());

            accountStore.Setup(s => s.Get(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(new Account());

            var context = new RegisterReportContext 
            { 
                AccountId = Guid.NewGuid(),
                CompanyId = Guid.NewGuid(),
                SupplierSystemId = "1", 
                RegisterId = "1" 
            };
            var generator = new RegisterReportGenerator(dateProvider.Object, invoiceStore.Object, 
                supplierStore.Object, accountStore.Object);

            var report = await generator.Generate(context);

            Assert.NotNull(report);
        }

        [Fact]
        public async Task Should_generate_x_report()
        {
            const string registerId = "1";
            
            var account = new Account();
            var supplier = new Supplier();
            var calculator = new InvoiceTotalsCalculator();
            var builder = new InvoiceBuilder();
            var director = new InvoiceDirector(builder, account.Id, supplier.SystemId);

            director.ConstructAnonymousWithSingleProductPaidWithCash();
            var invoice1 = builder.Result;
            invoice1.Totals = calculator.Calculate(invoice1);
            invoice1.RegisterId = registerId;

            builder.Reset();
            director.ConstructAnonymousWithSingleProductPaidWithCategory(100m, 1, 1.21m, PaymentCategory.Debit);
            var invoice2 = builder.Result;
            invoice2.Totals = calculator.Calculate(invoice2);
            invoice2.RegisterId = registerId;

            builder.Reset();
            director.ConstructReturn(invoice1);
            var invoice3 = builder.Result;
            invoice3.Totals = calculator.Calculate(invoice3);
            invoice3.RegisterId = registerId;

            builder.Reset();
            director.ConstructWithSettlement(100m, 1.17m, PaymentCategory.Credit, 0.10m);
            var invoice4 = builder.Result;
            invoice4.Totals = calculator.Calculate(invoice4);
            invoice4.RegisterId = registerId;

            var dateProvider = new Mock<RealLifeDateProvider>();
            var invoiceStore = new Mock<IInvoiceStore>();
            invoiceStore.Setup(i => i.List(It.IsAny<AuditCriteria>()))
                .ReturnsAsync(new List<Invoice>() 
                { 
                    invoice1, invoice2, invoice3, invoice4
                });

            var supplierStore = new Mock<ISupplierStore>();
            var accountStore = new Mock<IAccountStore>();

            supplierStore.Setup(s => s.Get(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(supplier);

            accountStore.Setup(s => s.Get(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(account);

            var context = new RegisterReportContext
            {
                AccountId = account.Id,
                CompanyId = account.CompanyId,
                SupplierSystemId = "1",
                RegisterId = registerId
            };
            var generator = new RegisterReportGenerator(dateProvider.Object, invoiceStore.Object,
                supplierStore.Object, accountStore.Object);

            var report = await generator.Generate(context);

            Assert.NotNull(report);
            Assert.True(report.Payments.Count == 3);
            Assert.True(report.Taxes.Count == 2);
            Assert.True(report.PaymentsPerEmployee.Count == 3);
            Assert.True(report.Products.Count == 1);
            Assert.True(report.Discount.Count == 1);
            Assert.True(report.Return.Amount == invoice3.Totals.Gross);
        }

        [Fact]
        public async Task Should_generate_z_report_when_period_is_closed()
        {
            var registerId = Guid.NewGuid();
            string registerIdString = registerId.ToString();
            var account = new Account();
            var supplier = new Supplier();
            var calculator = new InvoiceTotalsCalculator();
            var builder = new InvoiceBuilder();
            var director = new InvoiceDirector(builder, account.Id, supplier.SystemId);

            director.ConstructAnonymousWithSingleProductPaidWithCash();
            var invoice1 = builder.Result;
            invoice1.Totals = calculator.Calculate(invoice1);
            invoice1.RegisterId = registerIdString;

            var dateProvider = new Mock<RealLifeDateProvider>();
            var invoiceStore = new Mock<IInvoiceStore>();
            invoiceStore.Setup(i => i.List(It.IsAny<AuditCriteria>()))
                .ReturnsAsync(new List<Invoice>()
                {
                    invoice1,
                });
            var supplierStore = new Mock<ISupplierStore>();
            var accountStore = new Mock<IAccountStore>();

            supplierStore.Setup(s => s.Get(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(supplier);

            accountStore.Setup(s => s.Get(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(account);

            var generator = new RegisterReportGenerator(dateProvider.Object, invoiceStore.Object, 
                supplierStore.Object, accountStore.Object);

            var handler = new Mock<IHandlerChain<RegisterReport>>();
            var reportHandlerFactory = new Mock<IReportHandlerFactory>();
            reportHandlerFactory.Setup((s) => s.Create(It.IsAny<IReportComponentFactory>()))
                .Returns(handler.Object);
            
            var acfCollection = new Mock<IAccountComponentFactoryCollection>();
            var periodStore = new Mock<IPeriodStore>();


            var period = new Period { Supplier = supplier };
            period.Registers.Add(new Register { Id = registerId });

            var periodManager = new PeriodManager(generator, reportHandlerFactory.Object, acfCollection.Object,
                periodStore.Object, dateProvider.Object);

            await periodManager.ClosePeriod(period, account, period.Registers);
        }
    }
}
