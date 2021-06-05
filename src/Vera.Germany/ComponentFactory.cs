using System;
using System.Collections.Generic;
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
using Vera.Thermal;

namespace Vera.Germany
{
    public class ComponentFactory : IComponentFactory
    {
        private readonly IFiskalyClient _fiskalyClient;

        public ComponentFactory(Configuration configuration)
        {
            _fiskalyClient = new FiskalyClient(configuration.ApiKey, configuration.ApiSecret, configuration.BaseUrl);
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
            throw new NotImplementedException();
        }
    }
}
