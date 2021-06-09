using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Vera.Dependencies;
using Vera.Dependencies.Handlers;
using Vera.EventLogs;
using Vera.Models;
using Vera.Periods;
using Vera.Reports;
using Vera.Stores;
using Xunit;
using static Vera.Periods.PeriodClosingContext;

namespace Vera.Tests
{
    public class PeriodCloserTests
    {
        [Fact]
        public async Task Should_Call_IPeriodStore_To_Update_The_Period()
        {
            var account = new Account();
            var supplier = new Supplier();
            var registerId = Guid.NewGuid();
            var periodDate = DateTime.Now;
            var period = new Period { SupplierId = supplier.Id, Closing = periodDate };
            period.Registers.Add(new PeriodRegisterEntry { RegisterId = registerId, OpeningAmount = 10m });

            var supplierStore = new Mock<ISupplierStore>();
            var accountStore = new Mock<IAccountStore>();
            var periodStore = new Mock<IPeriodStore>();
            var eventLogStore = new Mock<IEventLogStore>();

            supplierStore.Setup(s => s.Get(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(supplier);

            accountStore.Setup(s => s.Get(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(account);

            periodStore.Setup(p => p.GetOpenPeriodForSupplier(It.IsAny<Guid>()))
                .Returns(Task.FromResult(period));

            periodStore.Setup((s) => s.Update(It.IsAny<Period>()))
                .Returns(Task.FromResult(period));

            eventLogStore.Setup(i => i.List(It.IsAny<EventLogCriteria>()))
                .ReturnsAsync(new List<EventLog>());

            var dateProvider = new Mock<IDateProvider>();

            var invoiceStore = new Mock<IInvoiceStore>();

            var generator = new RegisterReportGenerator(dateProvider.Object, invoiceStore.Object,
                accountStore.Object, periodStore.Object, eventLogStore.Object);

            var reportHandler = new Mock<IHandlerChain<RegisterReport>>();

            var periodCloser = new PeriodCloser(generator, periodStore.Object, dateProvider.Object);

            var registersToClose = new List<RegisterEntry>();
            registersToClose.Add(new RegisterEntry
            {
                Id = registerId,
                ClosingAmount = 5m
            });

            await periodCloser.ClosePeriod(reportHandler.Object,
                new PeriodClosingContext { Registers = registersToClose, Period = period, Account = account });

            var register = period.Registers.Single();

            periodStore.Verify(x => x.Update(It.Is<Period>(p => p.IsClosed && p.SupplierId == supplier.Id)));
        }

        [Fact]
        public async Task Should_Set_The_Closing_Property_To_Value_Of_IDateProvider()
        {
            var account = new Account();
            var supplier = new Supplier();
            var registerId = Guid.NewGuid();
            var periodDate = DateTime.Now;
            var period = new Period { SupplierId = supplier.Id };
            period.Registers.Add(new PeriodRegisterEntry { RegisterId = registerId, OpeningAmount = 10m });

            var supplierStore = new Mock<ISupplierStore>();
            var accountStore = new Mock<IAccountStore>();
            var periodStore = new Mock<IPeriodStore>();
            var eventLogStore = new Mock<IEventLogStore>();

            supplierStore.Setup(s => s.Get(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(supplier);

            accountStore.Setup(s => s.Get(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(account);

            periodStore.Setup(p => p.GetOpenPeriodForSupplier(It.IsAny<Guid>()))
                .Returns(Task.FromResult(period));

            periodStore.Setup((s) => s.Update(It.IsAny<Period>()))
                .Returns(Task.FromResult(period));

            eventLogStore.Setup(i => i.List(It.IsAny<EventLogCriteria>()))
                .ReturnsAsync(new List<EventLog>());

            var dateProvider = new Mock<IDateProvider>();
            dateProvider.SetupGet(d => d.Now).Returns(periodDate);

            var invoiceStore = new Mock<IInvoiceStore>();

            var generator = new RegisterReportGenerator(dateProvider.Object, invoiceStore.Object,
                accountStore.Object, periodStore.Object, eventLogStore.Object);

            var reportHandler = new Mock<IHandlerChain<RegisterReport>>();

            var periodCloser = new PeriodCloser(generator, periodStore.Object, dateProvider.Object);

            var registersToClose = new List<RegisterEntry>();
            registersToClose.Add(new RegisterEntry
            {
                Id = registerId,
                ClosingAmount = 5m
            });

            await periodCloser.ClosePeriod(reportHandler.Object,
                new PeriodClosingContext { Registers = registersToClose, Period = period, Account = account });

            var register = period.Registers.Single();

            periodStore.Verify(x => x.Update(It.Is<Period>(p => p.Closing == periodDate)));
        }

        [Fact]
        public async Task Should_Invoke_IRegisterReportGenerator_For_Every_Register_Thats_Being_Closed()
        {
            var account = new Account();
            var supplier = new Supplier();
            var register1Id = Guid.NewGuid();
            var register2Id = Guid.NewGuid();
            var periodDate = DateTime.Now;
            var period = new Period { SupplierId = supplier.Id, Closing = periodDate };
            period.Registers.Add(new PeriodRegisterEntry { RegisterId = register1Id, OpeningAmount = 10m });
            period.Registers.Add(new PeriodRegisterEntry { RegisterId = register2Id, OpeningAmount = 10m });

            var supplierStore = new Mock<ISupplierStore>();
            var accountStore = new Mock<IAccountStore>();
            var periodStore = new Mock<IPeriodStore>();
            var eventLogStore = new Mock<IEventLogStore>();

            supplierStore.Setup(s => s.Get(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(supplier);

            accountStore.Setup(s => s.Get(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(account);

            periodStore.Setup(p => p.GetOpenPeriodForSupplier(It.IsAny<Guid>()))
                .Returns(Task.FromResult(period));

            periodStore.Setup((s) => s.Update(It.IsAny<Period>()))
                .Returns(Task.FromResult(period));

            eventLogStore.Setup(i => i.List(It.IsAny<EventLogCriteria>()))
                .ReturnsAsync(new List<EventLog>());

            var dateProvider = new Mock<IDateProvider>();

            var invoiceStore = new Mock<IInvoiceStore>();

            var generator = new RegisterReportGenerator(dateProvider.Object, invoiceStore.Object,
                accountStore.Object, periodStore.Object, eventLogStore.Object);

            var reportHandler = new Mock<IHandlerChain<RegisterReport>>();

            var periodCloser = new PeriodCloser(generator, periodStore.Object, dateProvider.Object);

            var registersToClose = new List<RegisterEntry>();
            registersToClose.Add(new RegisterEntry
            {
                Id = register1Id,
                ClosingAmount = 5m
            });
            registersToClose.Add(new RegisterEntry
            {
                Id = register2Id,
                ClosingAmount = 5m
            });

            await periodCloser.ClosePeriod(reportHandler.Object,
                new PeriodClosingContext { Registers = registersToClose, Period = period, Account = account });

            reportHandler.Verify(x => x.Handle(It.Is<RegisterReport>(r => r.RegisterId == register1Id)));
            reportHandler.Verify(x => x.Handle(It.Is<RegisterReport>(r => r.RegisterId == register2Id)));
        }

        [Fact]
        public async Task Should_Set_The_Registers_ClosingAmount_In_The_Period_Thats_Being_Closed()
        {
            var account = new Account();
            var supplier = new Supplier();
            var registerId = Guid.NewGuid();
            var period = new Period { SupplierId = supplier.Id };
            period.Registers.Add(new PeriodRegisterEntry { RegisterId = registerId, OpeningAmount = 10m });

            var supplierStore = new Mock<ISupplierStore>();
            var accountStore = new Mock<IAccountStore>();
            var periodStore = new Mock<IPeriodStore>();
            var eventLogStore = new Mock<IEventLogStore>();

            supplierStore.Setup(s => s.Get(It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync(supplier);

            accountStore.Setup(s => s.Get(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(account);

            periodStore.Setup(p => p.GetOpenPeriodForSupplier(It.IsAny<Guid>()))
                .ReturnsAsync(period);

            periodStore.Setup((s) => s.Update(It.IsAny<Period>()))
                .Returns(Task.FromResult(new Period()));

            eventLogStore.Setup(i => i.List(It.IsAny<EventLogCriteria>()))
                .ReturnsAsync(new List<EventLog>());

            var dateProvider = new Mock<IDateProvider>();

            var invoiceStore = new Mock<IInvoiceStore>();

            var registerReportGenerator = new Mock<IRegisterReportGenerator>();
            registerReportGenerator.Setup(rrg => rrg.Generate(It.IsAny<RegisterReportContext>()))
                .Returns(Task.FromResult(new RegisterReport()));

            var reportHandler = new Mock<IHandlerChain<RegisterReport>>();

            var periodCloser = new PeriodCloser(registerReportGenerator.Object, periodStore.Object, dateProvider.Object);

            var registersToClose = new List<RegisterEntry>();
            registersToClose.Add(new RegisterEntry
            {
                Id = registerId,
                ClosingAmount = 5m
            });

            await periodCloser.ClosePeriod(reportHandler.Object,
                new PeriodClosingContext { Registers = registersToClose, Period = period, Account = account });

            var expectedRegister = new PeriodRegisterEntry
            {
                RegisterId = registerId,
                OpeningAmount = 10m,
                ClosingAmount = 5m,
            };

            var register = period.Registers.Single();

            Assert.Equal(expectedRegister.RegisterId, register.RegisterId);
            Assert.Equal(expectedRegister.OpeningAmount, register.OpeningAmount);
            Assert.Equal(expectedRegister.ClosingAmount, register.ClosingAmount);
        }
    }
}
