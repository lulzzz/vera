using Vera.Signing;

namespace Vera.Invoices
{
    public interface IInvoiceComponentFactory
    {
        IInvoiceValidator CreateInvoiceValidator();
        IInvoiceBucketGenerator CreateInvoiceBucketGenerator();
        IInvoiceNumberGenerator CreateInvoiceNumberGenerator();
        IPackageSigner CreatePackageSigner();
    }
}