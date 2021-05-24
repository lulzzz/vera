using System.Collections.Generic;
using System.Security.Cryptography;
using Vera.Audits;
using Vera.Configuration;
using Vera.Dependencies;
using Vera.Dependencies.Handlers;
using Vera.Invoices;
using Vera.Models;
using Vera.Norway.Audit;
using Vera.Registers;
using Vera.Reports;
using Vera.Signing;
using Vera.Stores;
using Vera.Thermal;

namespace Vera.Norway
{
    public sealed class ComponentFactory : IComponentFactory
    {
        private readonly RSA _rsa;
        private readonly Configuration _configuration;

        private readonly IReportStore _reportStore;

        public ComponentFactory(RSA rsa, Configuration configuration, IReportStore reportStore)
        {
            _rsa = rsa;
            _configuration = configuration;
            _reportStore = reportStore;
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
            return new AuditWriter(new RealLifeDateProvider(), _reportStore);
        }

        public IRegisterInitializer CreateRegisterInitializer()
        {
            return new OpenRegisterInitializer();
        }
    }
}
