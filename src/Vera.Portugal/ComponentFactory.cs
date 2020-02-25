using System.Security.Cryptography;
using Vera.Concurrency;
using Vera.Signing;

namespace Vera.Portugal
{
    public sealed class ComponentFactory : IComponentFactory
    {
        private readonly ILocker _locker;
        private readonly RSA _rsa;

        public ComponentFactory(ILocker locker, RSA rsa)
        {
            _locker = locker;
            _rsa = rsa;
        }

        public ILocker CreateLocker()
        {
            return _locker;
        }

        public IInvoiceBucketGenerator CreateInvoiceBucketGenerator()
        {
            return new InvoiceBucketGenerator();
        }

        public IInvoiceNumberGenerator CreateInvoiceNumberGenerator()
        {
            return new InvoiceNumberGenerator();
        }

        public IPackageSigner CreatePackageSigner()
        {
            return new PackageSigner(_rsa);
        }
    }
}