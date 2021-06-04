using Vera.Sweden.Models.Configs;

namespace Vera.Sweden.InfrasecHttpClient.Contracts
{
    public interface IInfrasecEnrollmentApiClientFactory
    {
        IInfrasecClient Create(SwedenConfigs swedenConfigs);
    }
}