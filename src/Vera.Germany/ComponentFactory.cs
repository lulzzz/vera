using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Vera.Audits;
using Vera.Configuration;
using Vera.Dependencies;
using Vera.Germany.Fiskaly;
using Vera.Germany.Invoices;
using Vera.Invoices;
using Vera.Models;
using Vera.Printing;
using Vera.Registers;
using Vera.Reports;
using Vera.Signing;
using Vera.Stores;
using Vera.Thermal;

namespace Vera.Germany
{
    public class ComponentFactory : IComponentFactory
    {
        private readonly IFiskalyClient _fiskalyClient;
        private readonly IPrintAuditTrailStore _printAuditTrailStore;
        private readonly ILoggerFactory _loggerFactory;

        public ComponentFactory(Configuration configuration, IPrintAuditTrailStore printAuditTrailStore, ILoggerFactory loggerFactory)
        {
            _fiskalyClient = new FiskalyClient(configuration.ApiKey, configuration.ApiSecret, configuration.BaseUrl);
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
            return new UniqueBucketPerSupplierGenerator();
        }

        public IInvoiceNumberGenerator CreateInvoiceNumberGenerator()
        {
            return new InvoiceSequenceNumberGenerator();
        }

        public IInvoiceSigner CreateInvoiceSigner()
        {
            return new InvoiceSigner(_fiskalyClient);
        }

        public IReportSigner CreateReportSigner()
        {
            return new NullReportSigner();
        }

        public IThermalReceiptGenerator CreateThermalReceiptGenerator()
        {
            //return new ThermalReceiptGenerator();
            throw new NotImplementedException();
        }

        public IReportReceiptGenerator CreateThermalReportGenerator()
        {
            //return new ReportReceiptGenerator();
            throw new NotImplementedException();
        }

        public IAuditWriter CreateAuditWriter()
        {
            //return new AuditWriter(new RealLifeDateProvider(), _reportStore);
            throw new NotImplementedException();
        }

        public IRegisterInitializer CreateRegisterInitializer()
        {
            return new GermanyRegisterInitializer(_fiskalyClient);
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
