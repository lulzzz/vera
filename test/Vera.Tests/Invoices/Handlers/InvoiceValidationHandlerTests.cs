using Moq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Vera.Invoices;
using Vera.Invoices.InvoiceValidators;
using Xunit;

namespace Vera.Tests.Invoices.Handlers
{
    public class InvoiceValidationHandlerTests
    {
        [Fact]
        public async Task Should_call_and_throw_validator1()
        {
            var line1 = new Models.InvoiceLine
            {
                UnitPrice = 1.99m,
                Quantity = 1,
                Taxes = new Models.Taxes
                {
                    Rate = 1.23m,
                    Category = Models.TaxesCategory.High
                }
            };
            var line2 = new Models.InvoiceLine
            {
                UnitPrice = 2.49m,
                Quantity = -1,
                Taxes = new Models.Taxes
                {
                    Rate = 1.23m,
                    Category = Models.TaxesCategory.High
                }
            };
            var invoice = new Models.Invoice
            {
                Lines = new List<Models.InvoiceLine>
                {
                    line1,
                    line2
                }
            };
            var errorMessage = "validation failed";

            var validator1 = new Mock<IInvoiceValidator>();
            validator1.Setup(a => a.Validate(It.IsAny<Models.Invoice>()))
                .Throws(new ValidationException(errorMessage));
            var validator2 = new Mock<IInvoiceValidator>();
            var validators = new List<IInvoiceValidator> { validator1.Object, validator2.Object };
            var validationHandler = new InvoiceValidationHandler(validators);

            var ex = await Assert.ThrowsAsync<ValidationException>(() => validationHandler.Handle(invoice));
            Assert.Equal(errorMessage, ex.Message);

            validator1.Verify(v => v.Validate(invoice), Times.Once());
            validator2.Verify(v => v.Validate(invoice), Times.Never());
        }

        [Fact]
        public async Task Should_pass_validation()
        {
            var line1 = new Models.InvoiceLine
            {
                UnitPrice = 1.99m,
                Quantity = 1,
                Taxes = new Models.Taxes
                {
                    Rate = 1.23m,
                    Category = Models.TaxesCategory.High
                }
            };
            var line2 = new Models.InvoiceLine
            {
                UnitPrice = 2.49m,
                Quantity = 1,
                Taxes = new Models.Taxes
                {
                    Rate = 1.23m,
                    Category = Models.TaxesCategory.High
                }
            };
            var invoice = new Models.Invoice
            {
                Lines = new List<Models.InvoiceLine>
                {
                    line1,
                    line2
                },
                Payments = new List<Models.Payment>
                {
                    new Models.Payment
                    {
                        Amount = 5.51m
                    }
                }
            };

            var validator1 = new MixedQuantitiesValidator();
            var validator2 = new TotalPaidValidator();
            var validators = new List<IInvoiceValidator> { validator1, validator2 };
            var validationHandler = new InvoiceValidationHandler(validators);
            var mockHandler = new InvoiceHandlersHelper().MockInvoiceHandler;
            validationHandler.WithNext(mockHandler.Object);

            await validationHandler.Handle(invoice);

            mockHandler.Verify(h => h.Handle(invoice));
        }
    }
}
