using System.Threading.Tasks;
using Vera.Concurrency;
using Vera.Configuration;
using Vera.Invoices;
using Vera.Signing;

namespace Vera
{
    public interface IComponentFactory
    {
        ILocker CreateLocker();
        IInvoiceBucketGenerator CreateInvoiceBucketGenerator();
        IInvoiceNumberGenerator CreateInvoiceNumberGenerator();
        IPackageSigner CreatePackageSigner();
    }
}