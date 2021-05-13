using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Vera.Dependencies.Handlers;
using Vera.Models;

namespace Vera.Invoices
{
    public class InvoiceValidationHandler : HandlerChain<Invoice>
    {
        private readonly IEnumerable<IInvoiceValidator> _validator;

        public InvoiceValidationHandler(IEnumerable<IInvoiceValidator> validator)
        {
            _validator = validator;
        }

        public override Task Handle(Invoice invoice)
        {
            foreach (var val in _validator)
            {
                var results = val.Validate(invoice);

                if (results.Any())
                {
                    throw new ValidationException(results.First().ErrorMessage);
                }
            }


            return base.Handle(invoice);
        }
    }
}