using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vera.Audits;
using Vera.Bootstrap;
using Vera.Dependencies;
using Vera.Dependencies.Handlers;
using Vera.EventLogs;
using Vera.Invoices;
using Vera.Models;
using Vera.Periods;
using Vera.Reports;
using Vera.Stores;
using Vera.Tests.Shared;
using Xunit;
using static Vera.Periods.PeriodCloser;

namespace Vera.Tests
{
    public class RegisterReportGeneratorTests
    {
        [Fact]
        public async Task Should_generate_empty_report()
        {
            var registerId = Guid.NewGuid();
            var period = new Period();
            period.Registers.Add(new PeriodRegisterEntry { RegisterId = registerId, OpeningAmount = 10m });

            var dateProvider = new Mock<RealLifeDateProvider>();
            var invoiceStore = new Mock<IInvoiceStore>();
            var accountStore = new Mock<IAccountStore>();
            var periodStore = new Mock<IPeriodStore>();
            var eventLogStore = new Mock<IEventLogStore>();

            invoiceStore.Setup(i => i.List(It.IsAny<AuditCriteria>()))
                .ReturnsAsync(new List<Invoice>());

            accountStore.Setup(s => s.Get(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(new Account());

            periodStore.Setup(p => p.GetOpenPeriodForSupplier(It.IsAny<Guid>()))
                .ReturnsAsync(period);

            eventLogStore.Setup(i => i.List(It.IsAny<EventLogCriteria>()))
                .ReturnsAsync(new List<EventLog>());

            var context = new RegisterReportContext
            {
                AccountId = Guid.NewGuid(),
                CompanyId = Guid.NewGuid(),
                SupplierId = Guid.NewGuid(),
                RegisterId = registerId
            };
            var generator = new RegisterReportGenerator(dateProvider.Object, invoiceStore.Object,
                accountStore.Object, periodStore.Object, eventLogStore.Object);

            var report = await generator.Generate(context);

            Assert.NotNull(report);
        }

        [Fact]
        public async Task Should_generate_x_report()
        {
            var account = new Account();
            var supplier = new Supplier();
            var registerId = Guid.NewGuid();
            var period = new Period();
            period.Registers.Add(new PeriodRegisterEntry { RegisterId = registerId, OpeningAmount = 10m });

            var calculator = new InvoiceTotalsCalculator();
            var builder = new InvoiceBuilder();
            var director = new InvoiceDirector(builder, account.Id, supplier.SystemId);

            director.ConstructAnonymousWithSingleProductPaidWithCash();
            var invoice1 = builder.Result;
            invoice1.Totals = calculator.Calculate(invoice1);
            invoice1.RegisterSystemId = registerId.ToString();

            builder.Reset();
            director.ConstructAnonymousWithSingleProductPaidWithCategory(100m, 1, 1.21m, PaymentCategory.Debit);
            var invoice2 = builder.Result;
            invoice2.Totals = calculator.Calculate(invoice2);
            invoice2.RegisterSystemId = registerId.ToString();

            builder.Reset();
            director.ConstructReturn(invoice1);
            var invoice3 = builder.Result;
            invoice3.Totals = calculator.Calculate(invoice3);
            invoice3.RegisterSystemId = registerId.ToString();

            builder.Reset();
            director.ConstructWithSettlement(100m, 1.17m, PaymentCategory.Credit, 0.10m);
            var invoice4 = builder.Result;
            invoice4.Totals = calculator.Calculate(invoice4);
            invoice4.RegisterSystemId = registerId.ToString();

            var dateProvider = new Mock<RealLifeDateProvider>();
            var invoiceStore = new Mock<IInvoiceStore>();
            invoiceStore.Setup(i => i.List(It.IsAny<AuditCriteria>()))
                .ReturnsAsync(new List<Invoice>()
                {
                    invoice1, invoice2, invoice3, invoice4
                });

            var accountStore = new Mock<IAccountStore>();
            var periodStore = new Mock<IPeriodStore>();
            var eventLogStore = new Mock<IEventLogStore>();

            accountStore.Setup(s => s.Get(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(account);

            periodStore.Setup(p => p.GetOpenPeriodForSupplier(It.IsAny<Guid>()))
                .ReturnsAsync(period);

            eventLogStore.Setup(i => i.List(It.IsAny<EventLogCriteria>()))
                .ReturnsAsync(new List<EventLog> { new EventLog() });

            var context = new RegisterReportContext
            {
                AccountId = account.Id,
                CompanyId = account.CompanyId,
                SupplierId = Guid.NewGuid(),
                RegisterId = registerId
            };
            var generator = new RegisterReportGenerator(dateProvider.Object, invoiceStore.Object,
                accountStore.Object, periodStore.Object, eventLogStore.Object);

            var report = await generator.Generate(context);

            Assert.NotNull(report);
            Assert.True(report.Payments.Count == 3);
            Assert.True(report.Taxes.Count == 2);
            Assert.True(report.PaymentsPerEmployee.Count == 3);
            Assert.True(report.Products.Count == 1);
            Assert.True(report.Discount.Count == 1);
            Assert.True(report.Return.Amount == invoice3.Totals.Gross);
            Assert.True(report.Type == RegisterReportType.Current);
            Assert.True(report.CashDrawerOpenings == 1);
            Assert.True(report.RegisterOpeningAmount == 10m);
        }

        // [Fact]
        // public async Task Should_generate_z_report_when_period_is_closed()
        // {
        //     var account = new Account();
        //     var supplier = new Supplier();
        //     var registerId = Guid.NewGuid();
        //     var period = new Period { SupplierId = supplier.Id };
        //     period.Registers.Add(new PeriodRegisterEntry { RegisterId = registerId, OpeningAmount = 10m });
        //
        //     var calculator = new InvoiceTotalsCalculator();
        //     var builder = new InvoiceBuilder();
        //     var director = new InvoiceDirector(builder, account.Id, supplier.SystemId);
        //
        //     director.ConstructAnonymousWithSingleProductPaidWithCash();
        //     var invoice1 = builder.Result;
        //     invoice1.Totals = calculator.Calculate(invoice1);
        //     invoice1.RegisterId = registerId.ToString();
        //
        //     var dateProvider = new Mock<RealLifeDateProvider>();
        //     var invoiceStore = new Mock<IInvoiceStore>();
        //     invoiceStore.Setup(i => i.List(It.IsAny<AuditCriteria>()))
        //         .ReturnsAsync(new List<Invoice>()
        //         {
        //             invoice1,
        //         });
        //     var accountStore = new Mock<IAccountStore>();
        //     var periodStore = new Mock<IPeriodStore>();
        //     var eventLogStore = new Mock<IEventLogStore>();
        //
        //     accountStore.Setup(s => s.Get(It.IsAny<Guid>(), It.IsAny<Guid>()))
        //         .ReturnsAsync(account);
        //
        //     periodStore.Setup(p => p.GetOpenPeriodForSupplier(It.IsAny<Guid>()))
        //         .ReturnsAsync(period);
        //
        //     eventLogStore.Setup(i => i.List(It.IsAny<EventLogCriteria>()))
        //         .ReturnsAsync(new List<EventLog>());
        //
        //     var generator = new RegisterReportGenerator(dateProvider.Object, invoiceStore.Object,
        //         accountStore.Object, periodStore.Object, eventLogStore.Object);
        //
        //     var handler = new Mock<IHandlerChain<RegisterReport>>();
        //     var reportHandlerFactory = new Mock<IReportHandlerFactory>();
        //     reportHandlerFactory.Setup((s) => s.Create(It.IsAny<IReportComponentFactory>()))
        //         .Returns(handler.Object);
        //
        //     var acfCollection = new Mock<IAccountComponentFactoryCollection>();
        //
        //     var periodManager = new PeriodCloser(generator, acfCollection.Object,
        //         periodStore.Object, dateProvider.Object);
        //
        //     await periodManager.ClosePeriod(new ClosePeriodModel
        //     {
        //         Period = period,
        //         Account = account,
        //         Registers = period.Registers
        //     });
        //
        //     handler.Verify(h =>
        //         h.Handle(It.Is<RegisterReport>(r =>
        //             r.ReportType == ReportType.Z && r.RegisterId == registerId.ToString())));
        // }
    }
}
