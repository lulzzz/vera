using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vera.Models;

namespace Vera.Invoices
{
    public interface IInvoiceValidator
    {
        IEnumerable<ValidationResult> Validate(Invoice invoice);
    }

    public class NullInvoiceValidator : IInvoiceValidator
    {
        public IEnumerable<ValidationResult> Validate(Invoice invoice) => new List<ValidationResult>();
    }
}