using System.Threading.Tasks;
using Vera.Concurrency;
using Vera.Signing;

namespace Vera
{
    public interface IComponentFactory
    {
        ILocker CreateLocker();
        IInvoiceBucketGenerator CreateInvoiceSequenceGenerator();
        IInvoiceNumberGenerator CreateInvoiceNumberGenerator();
        Task<IPackageSigner> CreatePackageSigner();
    }
}