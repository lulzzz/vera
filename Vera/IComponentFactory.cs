using System.Threading.Tasks;
using Vera.Signing;

namespace Vera
{
    public interface IComponentFactory
    {
        IInvoiceLocker CreateInvoiceLocker();
        IInvoiceSequenceGenerator CreateInvoiceSequenceGenerator();
        IInvoiceNumberGenerator CreateInvoiceNumberGenerator();
        Task<IPackageSigner> CreatePackageSigner();
    }
}