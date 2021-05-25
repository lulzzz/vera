using System.Collections.Generic;
using System.Security.Cryptography;
using Vera.Audits;
using Vera.Configuration;
using Vera.Dependencies;
using Vera.Invoices;
using Vera.Models;
using Vera.Portugal.Audits;
using Vera.Portugal.Invoices;
using Vera.Portugal.Stores;
using Vera.Registers;
using Vera.Reports;
using Vera.Signing;
using Vera.Thermal;

namespace Vera.Portugal
{
    public sealed class ComponentFactory : IComponentFactory
    {
        private readonly RSA _rsa;
        private readonly Configuration _configuration;
        private readonly IWorkingDocumentStore _wdStore;

        public ComponentFactory(RSA rsa, Configuration configuration, IWorkingDocumentStore wdStore)
        {
            _rsa = rsa;
            _configuration = configuration;
            _wdStore = wdStore;
        }

        public IConfigurationValidator CreateConfigurationValidator()
        {
            return new ConfigurationValidator();
        }

        public IEnumerable<IInvoiceValidator> CreateInvoiceValidators()
        {
            return new PortugalInvoiceValidators();
        }

        public IBucketGenerator<Invoice> CreateInvoiceBucketGenerator()
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
                new MachineReadableCodeGenerator(new ShortFormSignatureTransformer(), _configuration.CertificateNumber),
                _configuration.SocialCapital,
                _configuration.CertificateName,
                _configuration.CertificateNumber
            );
        }

        public IReportReceiptGenerator CreateThermalReportGenerator()
        {
            throw new System.NotImplementedException();
        }

        public IAuditWriter CreateAuditWriter()
        {
            return new AuditWriter(
                _configuration.ProductCompanyTaxId,
                _configuration.CertificateName,
                _configuration.CertificateNumber,
                _wdStore
            );
        }

        public IRegisterInitializer CreateRegisterInitializer()
        {
            return new SimpleRegisterInitializer();
        }

        public IRegisterCloser CreateRegisterCloser()
        {
            return new SimpleRegisterCloser();
        }
    }
}