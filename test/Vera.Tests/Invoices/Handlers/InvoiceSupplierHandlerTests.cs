using Moq;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Vera.Invoices;
using Vera.Stores;
using Xunit;

namespace Vera.Tests.Invoices.Handlers
{
    public class InvoiceSupplierHandlerTests
    {
        [Fact]
        public async Task Should_throw_supplier_systemId_missing()
        {
            var supplierStore = new Mock<ISupplierStore>();
            var invoice = new Models.Invoice
            {
                Supplier = new Models.Supplier()
            };
            var supplierHandler = new InvoiceSupplierHandler(supplierStore.Object);

            var ex = await Assert.ThrowsAsync<ValidationException>(() => supplierHandler.Handle(invoice));
            Assert.Equal("Missing supplier", ex.Message);
        }

        [Fact]
        public async Task Should_throw_supplier_not_exists()
        {
            const string supplierSystemId = "A1";
            var supplierStore = new Mock<ISupplierStore>();
            var invoice = new Models.Invoice
            {
                Supplier = new Models.Supplier { SystemId = supplierSystemId }
            };
            var supplierHandler = new InvoiceSupplierHandler(supplierStore.Object);

            var ex = await Assert.ThrowsAsync<ValidationException>(() => supplierHandler.Handle(invoice));
            Assert.Equal($"Unknown supplier '{supplierSystemId}'", ex.Message);
        }

        [Fact]
        public async Task Should_set_supplier_and_invoke_next()
        {
            const string supplierSystemId = "A1";
            var supplier = new Models.Supplier { SystemId = supplierSystemId };
            var invoice = new Models.Invoice
            {
                Supplier = supplier
            }; 
            var supplierStore = new Mock<ISupplierStore>();

            supplierStore.Setup(s => s.Get(It.IsAny<Guid>(), supplierSystemId))
                .ReturnsAsync(supplier);
            var supplierHandler = new InvoiceSupplierHandler(supplierStore.Object);

            await supplierHandler.Handle(invoice);
            var mockHandler = new InvoiceHandlersHelper().MockInvoiceHandler;
            supplierHandler.WithNext(mockHandler.Object);

            await supplierHandler.Handle(invoice);

            Assert.Equal(invoice.Supplier.SystemId, supplier.SystemId);

            mockHandler.Verify(h => h.Handle(invoice));
        }
    }
}
