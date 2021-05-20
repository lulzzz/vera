using Moq;
using Vera.Dependencies.Handlers;
using Vera.Models;

namespace Vera.Tests.Invoices.Handlers
{
    public class InvoiceHandlersHelper
    {
        public Mock<IHandlerChain<Invoice>> MockInvoiceHandler => new Mock<IHandlerChain<Invoice>>();
    }
}
