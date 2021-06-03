using System.Collections.Generic;
using Vera.Dependencies;
using Vera.Models;
using Vera.Signing;

namespace Vera.Invoices
{
    public interface IInvoiceComponentFactory
    {
        IEnumerable<IInvoiceValidator> CreateInvoiceValidators();
        IBucketGenerator<Invoice> CreateInvoiceBucketGenerator();
        IInvoiceNumberGenerator CreateInvoiceNumberGenerator();
        IInvoiceSigner CreateInvoiceSigner();
    }
}