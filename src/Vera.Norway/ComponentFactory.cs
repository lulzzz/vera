using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Vera.Audits;
using Vera.Configuration;
using Vera.Dependencies;
using Vera.Invoices;
using Vera.Signing;
using Vera.Thermal;

namespace Vera.Norway
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
            throw new NotImplementedException();
            //return new ConfigurationValidator();
        }

        public IInvoiceValidator CreateInvoiceValidator()
        {
            throw new NotImplementedException();
            //return new InvoiceValidator();
        }

        public IInvoiceBucketGenerator CreateInvoiceBucketGenerator()
        {
            throw new NotImplementedException();
            //return new InvoiceBucketGenerator();
        }

        public IInvoiceNumberGenerator CreateInvoiceNumberGenerator()
        {
            throw new NotImplementedException();
            //return new InvoiceNumberGenerator();
        }

        public IPackageSigner CreatePackageSigner()
        {
            throw new NotImplementedException();
            //use something else instead of PrivateKeyVersion or ignore it?
            //return new PackageSigner(_rsa, _configuration.PrivateKeyVersion);
        }

        public IThermalReceiptGenerator CreateThermalReceiptGenerator()
        {
            throw new NotImplementedException();
            //return new ThermalReceiptGenerator(
            //    _configuration.SocialCapital,
            //    _configuration.CertificateName,
            //    _configuration.CertificateNumber
            //);
        }

        public IAuditWriter CreateAuditWriter()
        {
            throw new NotImplementedException();
            //return new AuditWriter(
            //    _configuration.ProductCompanyTaxId,
            //    _configuration.CertificateName,
            //    _configuration.CertificateNumber
            //);
        }
    }
}
