using Vera.Models;

namespace Vera.Dependencies
{
    public interface IAccountComponentFactory
    {
        IComponentFactory Create(Account account);

        string Name { get; }
    }
}