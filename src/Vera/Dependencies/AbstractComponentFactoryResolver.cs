using Vera.Configuration;
using Vera.Models;

namespace Vera.Dependencies
{
    public abstract class AbstractComponentFactoryResolver<T> : IComponentFactoryResolver where T : AbstractAuditConfiguration, new()
    {
        private readonly IAccountConfigurationProvider _accountConfigurationProvider;

        protected AbstractComponentFactoryResolver(IAccountConfigurationProvider accountConfigurationProvider)
        {
            _accountConfigurationProvider = accountConfigurationProvider;
        }

        public IComponentFactory Resolve(Account account)
        {
            var config = _accountConfigurationProvider.GetConfiguration<T>(account);

            return Build(account, config);
        }

        protected abstract IComponentFactory Build(Account account, T configuration);

        // TODO: make this an enum in the Vera namespace instead?
        public abstract string Name { get; }
    }
}