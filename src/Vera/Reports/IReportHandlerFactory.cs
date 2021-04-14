using Microsoft.Extensions.Logging;
using Vera.Concurrency;
using Vera.Dependencies;
using Vera.Dependencies.Handlers;
using Vera.Models;
using Vera.Stores;

namespace Vera.Reports
{
    public interface IReportHandlerFactory
    {
        IHandlerChain<RegisterReport> Create(IReportComponentFactory factory);
    }

    public sealed class ReportHandlerFactory : IReportHandlerFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IReportStore _reportStore;
        private readonly IChainStore _chainStore;
        private readonly ILocker _locker;
        private readonly IBucketGenerator<RegisterReport> _reportBucketGenerator;

        public ReportHandlerFactory(ILoggerFactory loggerFactory,
            IReportStore reportStore,
            IChainStore chainStore,
            ILocker locker,
            IBucketGenerator<RegisterReport> reportBucketGenerator)
        {
            _loggerFactory = loggerFactory;
            _reportStore = reportStore;
            _chainStore = chainStore;
            _locker = locker;
            _reportBucketGenerator = reportBucketGenerator;
        }

        public IHandlerChain<RegisterReport> Create(IReportComponentFactory factory)
        {
            var persistenceHandler = new ReportPersistenceHandler(
                _loggerFactory.CreateLogger<ReportPersistenceHandler>(),
                _chainStore,
                _reportStore,
                factory.CreatePackageSigner(),
                _reportBucketGenerator
            );

            var head = new LockingHandler<RegisterReport>(persistenceHandler, _locker, _reportBucketGenerator);

            return head;
        }
    }
}
