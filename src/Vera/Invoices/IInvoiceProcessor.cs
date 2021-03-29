using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Vera.Concurrency;
using Vera.Models;
using Vera.Stores;

namespace Vera.Invoices
{
    public interface IInvoiceProcessor
    {
        /// <summary>
        /// Responsible for processing the invoice by:
        ///
        /// 1. Putting in the correct chain/bucket
        /// 2. Generating and assigning the correct invoice number
        /// 3. Generating and assigning a sequence
        /// 4. Signing the invoice and assigning it
        /// 5. Storing the invoice
        /// </summary>
        Task Process(IComponentFactory factory, Invoice invoice);
    }

    public sealed class InvoiceProcessor : IInvoiceProcessor
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IInvoiceStore _invoiceStore;
        private readonly IChainStore _chainStore;
        private readonly ILocker _locker;
        private readonly ISupplierStore _supplierStore;
        private readonly IPeriodStore _periodStore;

        public InvoiceProcessor(
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

        public async Task Process(IComponentFactory factory, Invoice invoice)
        {
            var persistenceHandler = new InvoicePersistenceHandler(
                _loggerFactory.CreateLogger<InvoicePersistenceHandler>(),
                _chainStore,
                _invoiceStore,
                factory.CreatePackageSigner(),
                factory.CreateInvoiceNumberGenerator(),
                factory.CreateInvoiceBucketGenerator()
            );

            var head = new InvoiceSupplierHandler(_supplierStore);
            
            head.WithNext(new InvoiceOpenPeriodHandler(_periodStore))
                .WithNext(new InvoiceTotalsHandler())
                .WithNext(new InvoiceValidationHandler(factory.CreateInvoiceValidator()))
                .WithNext(new InvoiceLockingHandler(persistenceHandler, factory.CreateInvoiceBucketGenerator(), _locker));

            await head.Handle(invoice);
        }
    }
}