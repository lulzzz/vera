using Microsoft.Extensions.Logging;
using Vera.Concurrency;
using Vera.Dependencies;
using Vera.Stores;

namespace Vera.Invoices
{
    public interface IInvoiceHandlerFactory
    {
        IInvoiceHandler Create(IInvoiceComponentFactory factory);
    }

    public sealed class InvoiceHandlerFactory : IInvoiceHandlerFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IInvoiceStore _invoiceStore;
        private readonly IChainStore _chainStore;
        private readonly ILocker _locker;
        private readonly ISupplierStore _supplierStore;
        private readonly IPeriodStore _periodStore;

        public InvoiceHandlerFactory(
            ILoggerFactory loggerFactory,
            IInvoiceStore invoiceStore,
            IChainStore chainStore,
            ILocker locker,
            ISupplierStore supplierStore, 
            IPeriodStore periodStore
        )
        {
            _loggerFactory = loggerFactory;
            _invoiceStore = invoiceStore;
            _chainStore = chainStore;
            _locker = locker;
            _supplierStore = supplierStore;
            _periodStore = periodStore;
        }

        public IInvoiceHandler Create(IInvoiceComponentFactory factory)
        {
            var head = new InvoiceSupplierHandler(_supplierStore);
         
            var persistenceHandler = new InvoicePersistenceHandler(
                _loggerFactory.CreateLogger<InvoicePersistenceHandler>(),
                _chainStore,
                _invoiceStore,
                factory.CreatePackageSigner(),
                factory.CreateInvoiceNumberGenerator(),
                factory.CreateInvoiceBucketGenerator()
            );
            
            head.WithNext(new InvoiceOpenPeriodHandler(_periodStore))
                .WithNext(new InvoiceTotalsHandler())
                .WithNext(new InvoiceValidationHandler(factory.CreateInvoiceValidator()))
                .WithNext(new InvoiceLockingHandler(persistenceHandler, factory.CreateInvoiceBucketGenerator(), _locker));

            return head;
        }
    }
}