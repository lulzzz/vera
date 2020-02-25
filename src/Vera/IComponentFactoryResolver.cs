namespace Vera
{
    public interface IComponentFactoryResolver
    {
        IComponentFactory Resolve(AccountConfig config);

        string Name { get; }
    }
}