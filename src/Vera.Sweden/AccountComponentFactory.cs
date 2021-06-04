using Vera.Dependencies;
using Vera.Stores;
using Vera.Sweden.InfrasecHttpClient.Contracts;
using Vera.Sweden.Models.Constants;
using Vera.Sweden.RequestBuilders.Contracts;
using Vera.Sweden.Validators.Contracts;

namespace Vera.Sweden
{
    public class AccountComponentFactory : AbstractAccountComponentFactory<Configuration>
    {
        private readonly IInfrasecEnrollmentApiClientFactory _infrasecEnrollmentApiClientFactory;
        private readonly IInfrasecNewStationEnrollmentRequestBuilder _infrasecNewStationEnrollmentRequestBuilder; 
        private readonly IInfrasecEnrollmentResponseValidator _infrasecEnrollmentResponseValidator; 
        private readonly IRegisterStore _registerStore;

        public AccountComponentFactory(IInfrasecEnrollmentApiClientFactory infrasecEnrollmentApiClientFactory, 
            IInfrasecNewStationEnrollmentRequestBuilder infrasecNewStationEnrollmentRequestBuilder, 
            IInfrasecEnrollmentResponseValidator infrasecEnrollmentResponseValidator, IRegisterStore registerStore)
        {
            _infrasecEnrollmentApiClientFactory = infrasecEnrollmentApiClientFactory;
            _infrasecNewStationEnrollmentRequestBuilder = infrasecNewStationEnrollmentRequestBuilder;
            _infrasecEnrollmentResponseValidator = infrasecEnrollmentResponseValidator;
            _registerStore = registerStore;
        }

        protected override IComponentFactory Create(Configuration configuration)
        {
            return new ComponentFactory(configuration, 
                _infrasecEnrollmentApiClientFactory, 
                _infrasecNewStationEnrollmentRequestBuilder, 
                _infrasecEnrollmentResponseValidator,
                _registerStore);
        }

        public override string Name => SwedenAuditingConstants.Identifier;
    }
}