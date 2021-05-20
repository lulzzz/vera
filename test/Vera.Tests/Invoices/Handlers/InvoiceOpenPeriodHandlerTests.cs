using Moq;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Vera.Invoices;
using Vera.Stores;
using Xunit;

namespace Vera.Tests.Invoices.Handlers
{
    public class InvoiceOpenPeriodHandlerTests
    {
        [Fact]
        public async Task Should_throw_not_open_period()
        {
            var periodStore = new Mock<IPeriodStore>();
            var invoice = new Models.Invoice
            {
                Supplier = new Models.Supplier()
            };
            var openPeriodHandler = new InvoiceOpenPeriodHandler(periodStore.Object);
            
            var ex = await Assert.ThrowsAsync<ValidationException>(() => openPeriodHandler.Handle(invoice));

            Assert.Equal("An open period is required", ex.Message);
        }


        [Fact]
        public async Task Should_throw_not_open_register()
        {
            var periodStore = new Mock<IPeriodStore>();
            var invoice = new Models.Invoice
            {
                Supplier = new Models.Supplier()
            };

            periodStore.Setup(s => s.GetOpenPeriodForSupplier(It.IsAny<Guid>()))
                .ReturnsAsync(new Models.Period());

            var openPeriodHandler = new InvoiceOpenPeriodHandler(periodStore.Object);

            var ex = await Assert.ThrowsAsync<ValidationException>(() => openPeriodHandler.Handle(invoice));

            Assert.Equal("An open register is required", ex.Message);
        }

        [Fact]
        public async Task Should_assign_the_current_open_period()
        {
            var registerId = Guid.NewGuid();
            var periodStore = new Mock<IPeriodStore>();
            var invoice = new Models.Invoice
            {
                Supplier = new Models.Supplier(),
                RegisterId = registerId.ToString()
            };
            var period = new Models.Period();
            period.Registers.Add(new Models.Register { Id = registerId });

            periodStore.Setup(s => s.GetOpenPeriodForSupplier(It.IsAny<Guid>()))
                .ReturnsAsync(period);

            var openPeriodHandler = new InvoiceOpenPeriodHandler(periodStore.Object);
            var mockHandler = new InvoiceHandlersHelper().MockInvoiceHandler;
            openPeriodHandler.WithNext(mockHandler.Object);

            await openPeriodHandler.Handle(invoice);

            mockHandler.Verify(h => h.Handle(invoice));
        }
    }
}
