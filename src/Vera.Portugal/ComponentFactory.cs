using System.Security.Cryptography;
using System.Threading.Tasks;
using Vera.Concurrency;
using Vera.Signing;

namespace Vera.Portugal
{
    public sealed class ComponentFactory : IComponentFactory
    {
        public ILocker CreateLocker()
        {
            // TODO(kevin): create blob locker
            throw new System.NotImplementedException();
        }

        public IInvoiceBucketGenerator CreateInvoiceSequenceGenerator()
        {
            return new InvoiceBucketGenerator();
        }

        public IInvoiceNumberGenerator CreateInvoiceNumberGenerator()
        {
            return new InvoiceNumberGenerator();
        }

        public Task<IPackageSigner> CreatePackageSigner()
        {
            // TODO(kevin): get input for RSA from somewhere
            return Task.FromResult<IPackageSigner>(new PackageSigner(RSA.Create()));
        }
    }
}