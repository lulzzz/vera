using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Vera.Models;

namespace Vera.Invoices
{
    public class InvoiceValidationHandler : InvoiceHandler
    {
        private readonly IInvoiceValidator _validator;

        public InvoiceValidationHandler(IInvoiceValidator validator)
        {
            _validator = validator;
        }

        public override Task Handle(Invoice invoice)
        {
            var results = _validator.Validate(invoice);

            if (results.Any())
            {
                throw new ValidationException(results.First().ErrorMessage);
            }
            
            return base.Handle(invoice);
        }
    }
}