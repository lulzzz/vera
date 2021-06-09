using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Vera.Audits;
using Vera.Configuration;
using Vera.Dependencies;
using Vera.Invoices;
using Vera.Models;
using Vera.Printing;
using Vera.Registers;
using Vera.Reports;
using Vera.Signing;
using Vera.Stores;
using Vera.Sweden.InfrasecHttpClient.Contracts;
using Vera.Sweden.RequestBuilders.Contracts;
using Vera.Sweden.Services;
using Vera.Sweden.Validators.Contracts;
using Vera.Thermal;

namespace Vera.Sweden
{
    public sealed class ComponentFactory : IComponentFactory
    {
        private readonly Configuration _configuration;
        private readonly IInfrasecEnrollmentApiClientFactory _infrasecEnrollmentApiClientFactory;
        private readonly IInfrasecNewStationEnrollmentRequestBuilder _infrasecNewStationEnrollmentRequestBuilder;
        private readonly IInfrasecEnrollmentResponseValidator _infrasecEnrollmentResponseValidator;
        private readonly IRegisterStore _registerStore;

        public ComponentFactory(Configuration configuration,
            IInfrasecEnrollmentApiClientFactory infrasecEnrollmentApiClientFactory,
            IInfrasecNewStationEnrollmentRequestBuilder infrasecNewStationEnrollmentRequestBuilder,
            IInfrasecEnrollmentResponseValidator infrasecEnrollmentResponseValidator,
            IRegisterStore registerStore)
        {
            _configuration = configuration;
            _infrasecEnrollmentApiClientFactory = infrasecEnrollmentApiClientFactory;
            _infrasecNewStationEnrollmentRequestBuilder = infrasecNewStationEnrollmentRequestBuilder;
            _infrasecEnrollmentResponseValidator = infrasecEnrollmentResponseValidator;
            _registerStore = registerStore;
        }

        public IConfigurationValidator CreateConfigurationValidator()
        {
            return new DefaultConfigurationValidator<Configuration>();
        }

        public IEnumerable<IInvoiceValidator> CreateInvoiceValidators()
        {
            return new List<IInvoiceValidator> { new NullInvoiceValidator() };
        }

        public IBucketGenerator<Invoice> CreateInvoiceBucketGenerator()
        {
            throw new System.NotImplementedException();
        }

        public IInvoiceNumberGenerator CreateInvoiceNumberGenerator()
        {
            throw new System.NotImplementedException();
        }

        public IInvoiceSigner CreateInvoiceSigner() => throw new System.NotImplementedException();

        public IThermalReceiptGenerator CreateThermalReceiptGenerator()
        {
            throw new System.NotImplementedException();
        }

        public IReportReceiptGenerator CreateThermalReportGenerator()
        {
            throw new System.NotImplementedException();
        }

        public IAuditWriter CreateAuditWriter()
        {
            throw new System.NotImplementedException();
        }

        public IRegisterInitializer CreateRegisterInitializer()
        {
            return new SyncNewStationWithInfrasecService(_infrasecEnrollmentApiClientFactory,
                _configuration.SwedenConfigs,
                _infrasecNewStationEnrollmentRequestBuilder,
                _infrasecEnrollmentResponseValidator,
                _registerStore);
        }

        public IRegisterCloser CreateRegisterCloser()
        {
            return new SwedenRegisterCloser();
        }

        public IThermalInvoicePrintActionFactory CreateThermalInvoicePrintActionFactory()
        {
             return new DoneThermalInvoicePrintActionFactory();
        }

        public IReportSigner CreateReportSigner() => throw new System.NotImplementedException();
    }
}
