using Vera.Models;

namespace Vera.Configuration
{
    public interface IAccountConfigurationProvider
    {
        T GetConfiguration<T>(Account account) where T : AbstractAuditConfiguration, new();
    }
}