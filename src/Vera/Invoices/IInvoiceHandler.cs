using System.Threading.Tasks;
using Vera.Models;

namespace Vera.Invoices
{
    public interface IInvoiceHandler
    {
        Task Handle(Invoice invoice); 
        IInvoiceHandler WithNext(IInvoiceHandler next);
    }
    
    public abstract class InvoiceHandler : IInvoiceHandler
    {
        private IInvoiceHandler _next;

        public virtual Task Handle(Invoice invoice)
        {
            return _next?.Handle(invoice) ?? Task.CompletedTask;
        }

        public IInvoiceHandler WithNext(IInvoiceHandler next)
        {
            _next = next;
            return next;
        }
    }
}