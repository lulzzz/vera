using Vera.Dependencies;
using Vera.Models;
using Vera.Signing;

namespace Vera.Invoices
{
    public interface IInvoiceComponentFactory
    {
        IInvoiceValidator CreateInvoiceValidator();
        IBucketGenerator<Invoice> CreateInvoiceBucketGenerator();
        IInvoiceNumberGenerator CreateInvoiceNumberGenerator();
        IPackageSigner CreatePackageSigner();
    }
}