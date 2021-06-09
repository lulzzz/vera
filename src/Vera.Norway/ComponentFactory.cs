using System.Collections.Generic;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Vera.Audits;
using Vera.Configuration;
using Vera.Dependencies;
using Vera.Invoices;
using Vera.Models;
using Vera.Norway.Audit;
using Vera.Printing;
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
        private readonly IPrintAuditTrailStore _printAuditTrailStore;
        private readonly ILoggerFactory _loggerFactory;

        public ComponentFactory(RSA rsa, Configuration configuration, IReportStore reportStore, IPrintAuditTrailStore printAuditTrailStore, ILoggerFactory loggerFactory)
        {
            _rsa = rsa;
            _configuration = configuration;
            _reportStore = reportStore;
            _printAuditTrailStore = printAuditTrailStore;
            _loggerFactory = loggerFactory;
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

        public IInvoiceSigner CreateInvoiceSigner()
        {
            return new InvoiceSigner(_rsa, _configuration.PrivateKeyVersion);
        }

        public IReportSigner CreateReportSigner()
        {
            return new NullReportSigner();
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
            return new SimpleRegisterInitializer();
        }

        public IRegisterCloser CreateRegisterCloser()
        {
            return new SimpleRegisterCloser();
        }
        
        public IThermalInvoicePrintActionFactory CreateThermalInvoicePrintActionFactory()
        {
            var thermalReceiptGenerator = CreateThermalReceiptGenerator();

            var logger = _loggerFactory.CreateLogger<EscPosInvoicePrintActionFactory>();

            return new EscPosInvoicePrintActionFactory(logger, thermalReceiptGenerator, _printAuditTrailStore);
        }
    }
}
