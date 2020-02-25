using System.Security.Cryptography;
using Vera.Concurrency;

namespace Vera.Portugal
{
    public class ComponentFactoryResolver : IComponentFactoryResolver
    {
        private readonly ILocker _locker;

        public ComponentFactoryResolver(ILocker locker)
        {
            _locker = locker;
        }

        public IComponentFactory Resolve(AccountConfig config)
        {
            // TODO(kevin): get the RSA key and pass it to the factory

            return new ComponentFactory(_locker, RSA.Create());
        }

        public string Name => "PT";
    }
}