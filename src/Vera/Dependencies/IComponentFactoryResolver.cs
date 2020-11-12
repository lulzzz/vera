using Vera.Models;

namespace Vera.Dependencies
{
    public interface IComponentFactoryResolver
    {
        IComponentFactory Resolve(Account account);

        string Name { get; }
    }
}