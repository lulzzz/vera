using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Vera.Models;

namespace Vera.Stores
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