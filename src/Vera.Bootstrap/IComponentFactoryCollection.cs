using System;
using System.Collections.Generic;
using System.Linq;
using Vera.Dependencies;
using Vera.Models;

namespace Vera.Bootstrap
{
    public interface IComponentFactoryCollection
    {
        IComponentFactory Get(Account account);
    }

    public sealed class ComponentFactoryCollection : IComponentFactoryCollection
    {
        private readonly IEnumerable<IComponentFactoryResolver> _resolvers;

        public ComponentFactoryCollection(IEnumerable<IComponentFactoryResolver> resolvers)
        {
            _resolvers = resolvers;
        }

        public IComponentFactory Get(Account account)
        {
            var factory = _resolvers.FirstOrDefault(x => x.Name == account.Certification);

            if (factory == null)
            {
                throw new InvalidOperationException(
                    $"No component factory available for certification {account.Certification}");
            }

            return factory.Resolve(account);
        }
    }
}