using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Vera.Concurrency;
using Vera.Signing;

namespace Vera.Portugal
{
    public sealed class ComponentFactory : IComponentFactory
    {
        private readonly string _blobConnectionString;
        private readonly RSA _rsa;

        public ComponentFactory(string blobConnectionString, RSA rsa)
        {
            _blobConnectionString = blobConnectionString;
            _rsa = rsa;
        }

        public ILocker CreateLocker()
        {
            return new AzureBlobLocker(_blobConnectionString);
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