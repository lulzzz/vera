using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Vera.Concurrency;
using Vera.Models;
using Vera.Signing;
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
        private readonly ILogger<InvoiceProcessor> _logger;
        private readonly IInvoiceStore _invoiceStore;
        private readonly IChainStore _chainStore;
        private readonly ILocker _locker;
        private readonly InvoiceTotalsCalculator _invoiceTotalsCalculator;

        public InvoiceProcessor(
            ILogger<InvoiceProcessor> logger,
            IInvoiceStore invoiceStore,
            IChainStore chainStore,
            ILocker locker)
        {
            _logger = logger;
            _invoiceStore = invoiceStore;
            _chainStore = chainStore;
            _locker = locker;
            _invoiceTotalsCalculator = new InvoiceTotalsCalculator();
        }

        public async Task Process(IComponentFactory factory, Invoice invoice)
        {
            var bucket = factory.CreateInvoiceBucketGenerator().Generate(invoice);

            // Lock on the unique sequence of the invoice so no other invoice can enter
            // the bucket to ensure the sequence stays in-order
            await using (await _locker.Lock(bucket, TimeSpan.FromSeconds(15)))
            {
                var packageSigner = factory.CreatePackageSigner();

                var chainContext = new ChainContext(invoice.AccountId, bucket);
                
                // Get last stored invoice based on the bucket for the invoice
                var last = await _chainStore.Last(chainContext);

                invoice.Totals = _invoiceTotalsCalculator.Calculate(invoice);
                invoice.Sequence = last.NextSequence;
                invoice.Number = await factory.CreateInvoiceNumberGenerator().Generate(invoice);
                invoice.Signature = await packageSigner.Sign(new Package(invoice, last.Signature));

                await _invoiceStore.Store(invoice);

                try
                {
                    await last.Append(invoice.Signature);
                }
                catch (Exception chainException)
                {
                    // Failed to append to the chain, means we have to
                    // rollback the invoice because it's not stored in a chain
                    _logger.LogError(chainException, $"failed to append to chain {chainContext}, deleting created invoice {invoice.Id}");

                    try
                    {
                        await _invoiceStore.Delete(invoice);
                        _logger.LogInformation("successfully removed invoice, chain has been restored");
                    }
                    catch (Exception invoiceException)
                    {
                        // TODO(kevin): this means the chain is now in an invalid state
                        // ^ how do we recover from this?
                        _logger.LogError(invoiceException, "failed to delete invoice after appending to the chain failed");
                    }
                }
            }
        }
    }
}