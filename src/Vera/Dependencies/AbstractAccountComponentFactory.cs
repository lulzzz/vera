using Vera.Configuration;
using Vera.Models;

namespace Vera.Dependencies
{
    public abstract class AbstractAccountComponentFactory<T> : IAccountComponentFactory where T : AbstractAccountConfiguration, new()
    {
        public IComponentFactory CreateComponentFactory(Account account)
        {
            var config = new T();

            if (account.Configuration != null)
            {
                config.Initialize(account.Configuration);
            }

            return Create(config);
        }

        protected abstract IComponentFactory Create(T configuration);

        public abstract string Name { get; }
    }
}