using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vera.Models;

namespace Vera.Invoices
{
    public interface IInvoiceValidator
    {
        ICollection<ValidationResult> Validate(Invoice invoice);
    }

    public class NullInvoiceValidator : IInvoiceValidator
    {
        public ICollection<ValidationResult> Validate(Invoice invoice) => new List<ValidationResult>();
    }
}