using Microsoft.Extensions.Logging;
using Vera.Concurrency;
using Vera.Dependencies.Handlers;
using Vera.Invoices;
using Vera.Models;
using Vera.Portugal.WorkingDocuments;
using Vera.Portugal.Stores;
using Vera.Stores;
using Vera.Dependencies;

namespace Vera.Portugal.Invoices
{
    public sealed class PortugalInvoiceHandlerFactory : IInvoiceHandlerFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IInvoiceStore _invoiceStore;
        private readonly IChainStore _chainStore;
        private readonly ILocker _locker;
        private readonly ISupplierStore _supplierStore;
        private readonly IPeriodStore _periodStore;
        private readonly IWorkingDocumentStore _wdStore;
        private readonly IGrandTotalAuditTrailStore _grandTotalAuditTrailStore;

        public PortugalInvoiceHandlerFactory(
            ILoggerFactory loggerFactory,
            IInvoiceStore invoiceStore,
            IChainStore chainStore,
            ILocker locker,
            ISupplierStore supplierStore,
            IPeriodStore periodStore,
            IWorkingDocumentStore wdStore,
            IGrandTotalAuditTrailStore grandTotalAuditTrailStore
        )
        {
            _loggerFactory = loggerFactory;
            _invoiceStore = invoiceStore;
            _chainStore = chainStore;
            _locker = locker;
            _supplierStore = supplierStore;
            _periodStore = periodStore;
            _wdStore = wdStore;
            _grandTotalAuditTrailStore = grandTotalAuditTrailStore;
        }

        public IHandlerChain<Invoice> Create(IComponentFactory factory)
        {
            var signer = factory.CreateInvoiceSigner();

            var bucketGenerator = factory.CreateInvoiceBucketGenerator();
            var head = new InvoiceSupplierHandler(_supplierStore);

            var wdHandler = new WorkingDocumentsHandler(
                _wdStore,
                _chainStore,
                signer,
                _loggerFactory.CreateLogger<WorkingDocumentsHandler>()
            );

            var persistenceHandler = new InvoicePersistenceHandler(
                _loggerFactory.CreateLogger<InvoicePersistenceHandler>(),
                _chainStore,
                _invoiceStore,
                signer,
                factory.CreateInvoiceNumberGenerator(),
                bucketGenerator,
                factory.CreateGrandTotalAuditTrailBucketGenerator(),
                _grandTotalAuditTrailStore
            );

            wdHandler.WithNext(persistenceHandler);

            head.WithNext(new InvoiceOpenPeriodHandler(_periodStore))
                .WithNext(new InvoiceTotalsHandler())
                .WithNext(new InvoiceValidationHandler(factory.CreateInvoiceValidators()))
                .WithNext(new LockingHandler<Invoice>(wdHandler, _locker, bucketGenerator));

            return head;
        }

        public string Name => "PT";
    }
}
