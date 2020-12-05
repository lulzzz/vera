using Vera.Models;

namespace Vera.Configuration
{
    public class AccountConfigurationProvider : IAccountConfigurationProvider
    {
        public T GetConfiguration<T>(Account account) where T : AbstractAuditConfiguration, new()
        {
            if (account.Configuration == null)
            {
                return new T();
            }

            var config = new T();
            config.Initialize(account.Configuration);

            return config;
        }
    }
}