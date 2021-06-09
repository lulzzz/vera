using Microsoft.Extensions.Logging;
using Vera.Dependencies;
using Vera.Printing;
using Vera.Stores;

namespace Vera.Germany
{
    public class AccountComponentFactory : AbstractAccountComponentFactory<Configuration>
    {
        private readonly IPrintAuditTrailStore _printAuditTrailStore;
        private readonly ILoggerFactory _loggerFactory;

        public AccountComponentFactory(IPrintAuditTrailStore printAuditTrailStore, ILoggerFactory loggerFactory)
        {
            _printAuditTrailStore = printAuditTrailStore;
            _loggerFactory = loggerFactory;
        }


        protected override IComponentFactory Create(Configuration config)
        {
            return new ComponentFactory(config, _printAuditTrailStore, _loggerFactory);
        }

        public override string Name => "DE";
    }
}
