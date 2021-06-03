using Vera.Dependencies;

namespace Vera.Germany
{
    public class AccountComponentFactory : AbstractAccountComponentFactory<Configuration>
    {

        protected override IComponentFactory Create(Configuration config)
        {
            return new ComponentFactory(config);
        }

        public override string Name => "DE";
    }
}
