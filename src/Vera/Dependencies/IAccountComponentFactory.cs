using Vera.Models;

namespace Vera.Dependencies
{
    public interface IAccountComponentFactory
    {
        IComponentFactory CreateComponentFactory(Account account);

        string Name { get; }
    }
}