using System.Collections.Generic;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Vera.Audits;
using Vera.Configuration;
using Vera.Dependencies;
using Vera.Invoices;
using Vera.Models;
using Vera.Portugal.Audits;
using Vera.Portugal.Invoices;
using Vera.Portugal.Stores;
using Vera.Printing;
using Vera.Registers;
using Vera.Reports;
using Vera.Signing;
using Vera.Stores;
using Vera.Thermal;

namespace Vera.Portugal
{
    public sealed class ComponentFactory : IComponentFactory
    {
        private readonly RSA _rsa;
        private readonly Configuration _configuration;
        private readonly IWorkingDocumentStore _wdStore;
        private readonly IPrintAuditTrailStore _printAuditTrailStore;
        private readonly ILoggerFactory _loggerFactory;

        public ComponentFactory(RSA rsa, Configuration configuration, IWorkingDocumentStore wdStore, IPrintAuditTrailStore printAuditTrailStore, ILoggerFactory loggerFactory)
        {
            _rsa = rsa;
            _configuration = configuration;
            _wdStore = wdStore;
            _printAuditTrailStore = printAuditTrailStore;
            _loggerFactory = loggerFactory;
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

        public IBucketGenerator<Invoice> CreateGrandTotalAuditTrailBucketGenerator()
        {
            return new GrandTotalAuditTrailBucketGenerator();
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
        
        public IThermalInvoicePrintActionFactory CreateThermalInvoicePrintActionFactory()
        {
            var thermalReceiptGenerator = CreateThermalReceiptGenerator();

            var logger = _loggerFactory.CreateLogger<EscPosInvoicePrintActionFactory>();

            return new EscPosInvoicePrintActionFactory(logger, thermalReceiptGenerator, _printAuditTrailStore);
        }
    }
}
