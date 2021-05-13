using System.Collections.Generic;
using System.Security.Cryptography;
using Vera.Audits;
using Vera.Configuration;
using Vera.Dependencies;
using Vera.Invoices;
using Vera.Models;
using Vera.Reports;
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
            return new ConfigurationValidator();
        }

        public IEnumerable<IInvoiceValidator> CreateInvoiceValidators()
        {
            return new List<IInvoiceValidator> { new NullInvoiceValidator() };
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
            return new ThermalReceiptGenerator();
        }

        public IReportReceiptGenerator CreateThermalReportGenerator()
        {
            return new ReportReceiptGenerator();
        }

        public IAuditWriter CreateAuditWriter()
        {
            return new AuditWriter(new RealLifeDateProvider());
        }
    }
}
