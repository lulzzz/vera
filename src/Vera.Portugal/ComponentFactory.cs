using System.Security.Cryptography;
using Vera.Audit;
using Vera.Concurrency;
using Vera.Invoices;
using Vera.Portugal.Models;
using Vera.Signing;
using Vera.Stores;
using Vera.Thermal;

namespace Vera.Portugal
{
    public sealed class ComponentFactory : IComponentFactory
    {
        private readonly IInvoiceStore _invoiceStore;
        private readonly ILocker _locker;
        private readonly RSA _rsa;
        private readonly Configuration _configuration;

        public ComponentFactory(IInvoiceStore invoiceStore, ILocker locker, RSA rsa, Configuration configuration)
        {
            _invoiceStore = invoiceStore;
            _locker = locker;
            _rsa = rsa;
            _configuration = configuration;
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

        public IThermalReceiptGenerator CreateThermalReceiptGenerator()
        {
            return new ThermalReceiptGenerator(
                _configuration.SocialCapital,
                _configuration.CertificateName,
                _configuration.CertificateNumber
            );
        }
    }
}