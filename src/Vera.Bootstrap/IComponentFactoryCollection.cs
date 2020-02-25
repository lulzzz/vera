using System.Collections.Generic;
using System.Linq;

namespace Vera.Bootstrap
{
    public interface IComponentFactoryCollection
    {
        IComponentFactory Get(AccountConfig config);
    }

    public sealed class ComponentFactoryCollection : IComponentFactoryCollection
    {
        private readonly IEnumerable<IComponentFactoryResolver> _resolvers;

        public ComponentFactoryCollection(IEnumerable<IComponentFactoryResolver> resolvers)
        {
            _resolvers = resolvers;
        }

        public IComponentFactory Get(AccountConfig config)
        {
            return _resolvers.Single(x => x.Name == config.Name).Resolve(config);
        }
    }
}