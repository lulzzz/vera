using Microsoft.Extensions.Logging;
using Vera.Concurrency;
using Vera.Dependencies;
using Vera.Dependencies.Handlers;
using Vera.EventLogs;
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
        private readonly ISupplierStore _supplierStore;
        private readonly IEventLogStore _eventLogStore;
        private readonly ILocker _locker;
        private readonly IDateProvider _dateProvider;
        private readonly IBucketGenerator<RegisterReport> _reportBucketGenerator;

        public ReportHandlerFactory(ILoggerFactory loggerFactory,
            IReportStore reportStore,
            IChainStore chainStore,
            ISupplierStore supplierStore,
            IEventLogStore eventLogStore,
            ILocker locker,
            IDateProvider dateProvider,
            IBucketGenerator<RegisterReport> reportBucketGenerator)
        {
            _loggerFactory = loggerFactory;
            _reportStore = reportStore;
            _chainStore = chainStore;
            _supplierStore = supplierStore;
            _eventLogStore = eventLogStore;
            _locker = locker;
            _dateProvider = dateProvider;
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

            persistenceHandler.WithNext(new RegisterReportEventLogHandler(
                _eventLogStore, _supplierStore, _dateProvider));

            var head = new LockingHandler<RegisterReport>(persistenceHandler, _locker, _reportBucketGenerator);

            return head;
        }
    }
}
