using System.Security.Cryptography;
using Vera.Audits;
using Vera.Configuration;
using Vera.Invoices;
using Vera.Signing;
using Vera.Thermal;

namespace Vera.Portugal
{
    public sealed class ComponentFactory : IComponentFactory
    {
        private readonly RSA _rsa;
        private readonly Configuration _configuration;

        public ComponentFactory(RSA rsa, Configuration configuration)
        {
            _rsa = rsa;
            _configuration = configuration;
        }

        public IConfigurationValidator CreateConfigurationValidator()
        {
            return new ConfigurationValidator();
        }

        public IInvoiceValidator CreateInvoiceValidator()
        {
            return new InvoiceValidator();
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
            return new PackageSigner(_rsa, _configuration.PrivateKeyVersion);
        }

        public IThermalReceiptGenerator CreateThermalReceiptGenerator()
        {
            return new ThermalReceiptGenerator(
                _configuration.SocialCapital,
                _configuration.CertificateName,
                _configuration.CertificateNumber
            );
        }

        public IAuditWriter CreateAuditWriter()
        {
            return new AuditWriter(
                _configuration.ProductCompanyTaxId,
                _configuration.CertificateName,
                _configuration.CertificateNumber
            );
        }
    }
}