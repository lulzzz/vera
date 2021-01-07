using Vera.Configuration;
using Vera.Models;

namespace Vera.Dependencies
{
    public interface IAccountComponentFactory
    {
        IComponentFactory CreateComponentFactory(Account account);
        AbstractAccountConfiguration CreateConfiguration();

        string Name { get; }
    }
}