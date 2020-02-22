using System.Threading.Tasks;
using Vera.Concurrency;
using Vera.Signing;

namespace Vera
{
    public interface IComponentFactory
    {
        ILocker CreateLocker();
        IInvoiceSequenceGenerator CreateInvoiceSequenceGenerator();
        IInvoiceNumberGenerator CreateInvoiceNumberGenerator();
        Task<IPackageSigner> CreatePackageSigner();
    }
}