using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Vera.Dependencies;
using Vera.Models;

namespace Vera.Bootstrap
{
    public interface IAccountComponentFactoryCollection
    {
        /// <summary>
        /// Attempts to retrieve the <c>IComponentFactory</c> for the given account based on the active certification.
        /// This method will throw if no factories can be found.
        /// </summary>
        /// <param name="account">account to resolve the factory for</param>
        /// <returns>factory based on the given account</returns>
        IComponentFactory GetComponentFactory(Account account);

        /// <summary>
        /// Returns the names of the supported certifications that accounts can make use of.
        /// </summary>
        ICollection<string> Names { get; }
    }

    public sealed class AccountComponentFactoryCollection : IAccountComponentFactoryCollection
    {
        private readonly IDictionary<string, IAccountComponentFactory> _factories;

        public AccountComponentFactoryCollection(IEnumerable<IAccountComponentFactory> factories)
        {
            _factories = factories.ToImmutableDictionary(x => x.Name);
        }

        public IComponentFactory GetComponentFactory(Account account)
        {
            return GetOrThrow(account).CreateComponentFactory(account);
        }

        public ICollection<string> Names => _factories.Keys;

        private IAccountComponentFactory GetOrThrow(Account account)
        {
            if (_factories.TryGetValue(account.Certification, out var factory))
            {
                return factory;
            }

            throw new InvalidOperationException($"No component factory available for certification {account.Certification}");
        }
    }
}