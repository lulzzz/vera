using Microsoft.Extensions.Logging;
using Vera.Concurrency;
using Vera.Dependencies;
using Vera.Dependencies.Handlers;
using Vera.Models;
using Vera.Stores;

namespace Vera.Invoices
{
    public interface IInvoiceHandlerFactory
    {
        IHandlerChain<Invoice> Create(IComponentFactory factory);
        string Name { get; }
    }

    public sealed class InvoiceHandlerFactory : IInvoiceHandlerFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IInvoiceStore _invoiceStore;
        private readonly IChainStore _chainStore;
        private readonly ILocker _locker;
        private readonly ISupplierStore _supplierStore;
        private readonly IPeriodStore _periodStore;
        private readonly IGrandTotalAuditTrailStore _grandTotalAuditTrailStore;

        public InvoiceHandlerFactory(
            ILoggerFactory loggerFactory,
            IInvoiceStore invoiceStore,
            IChainStore chainStore,
            ILocker locker,
            ISupplierStore supplierStore,
            IPeriodStore periodStore,
            IGrandTotalAuditTrailStore grandTotalAuditTrailStore
        )
        {
            _loggerFactory = loggerFactory;
            _invoiceStore = invoiceStore;
            _chainStore = chainStore;
            _locker = locker;
            _supplierStore = supplierStore;
            _periodStore = periodStore;
            _grandTotalAuditTrailStore = grandTotalAuditTrailStore;
        }

        public IHandlerChain<Invoice> Create(IComponentFactory factory)
        {
            var head = new InvoiceSupplierHandler(_supplierStore);

            var persistenceHandler = new InvoicePersistenceHandler(
                _loggerFactory.CreateLogger<InvoicePersistenceHandler>(),
                _chainStore,
                _invoiceStore,
                factory.CreateInvoiceSigner(),
                factory.CreateInvoiceNumberGenerator(),
                factory.CreateInvoiceBucketGenerator(),
                factory.CreateGrandTotalAuditTrailBucketGenerator(),
                _grandTotalAuditTrailStore
            );

            head.WithNext(new InvoiceOpenPeriodHandler(_periodStore))
                .WithNext(new InvoiceTotalsHandler())
                .WithNext(new InvoiceValidationHandler(factory.CreateInvoiceValidators()))
                .WithNext(new LockingHandler<Invoice>(persistenceHandler, _locker, factory.CreateInvoiceBucketGenerator()));

            return head;
        }

        public string Name => "default";
    }
}
