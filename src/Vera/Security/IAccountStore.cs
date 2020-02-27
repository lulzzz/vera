using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Vera.Security
{
    public interface IAccountStore
    {
        Task Store(Account account);
    }

    public class CosmosAccountStore : IAccountStore
    {
        private readonly Container _container;

        public CosmosAccountStore(Container container)
        {
            _container = container;
        }

        public Task Store(Account account)
        {
            return _container.CreateItemAsync(account, PartitionKey.None);
        }
    }
}