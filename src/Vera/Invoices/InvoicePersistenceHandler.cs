using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Vera.Dependencies;
using Vera.Dependencies.Handlers;
using Vera.Models;
using Vera.Signing;
using Vera.Stores;

namespace Vera.Invoices
{
    public class InvoicePersistenceHandler : HandlerChain<Invoice>
    {
        private readonly ILogger<InvoicePersistenceHandler> _logger;
        private readonly IChainStore _chainStore;
        private readonly IInvoiceStore _invoiceStore;
        private readonly IInvoiceSigner _signer;
        private readonly IInvoiceNumberGenerator _invoiceNumberGenerator;
        private readonly IBucketGenerator<Invoice> _invoiceBucketGenerator;
        private readonly IBucketGenerator<Invoice> _grandTotalAuditTrailBucketGenerator;
        private readonly IGrandTotalAuditTrailStore _grandTotalAuditTrailStore;

        public InvoicePersistenceHandler(
            ILogger<InvoicePersistenceHandler> logger,
            IChainStore chainStore,
            IInvoiceStore invoiceStore, 
            IInvoiceSigner signer,
            IInvoiceNumberGenerator invoiceNumberGenerator,
            IBucketGenerator<Invoice> invoiceBucketGenerator,
            IBucketGenerator<Invoice> grandTotalAuditTrailBucketGenerator,
            IGrandTotalAuditTrailStore grandTotalAuditTrailStore
        )
        {
            _logger = logger;
            _chainStore = chainStore;
            _invoiceStore = invoiceStore;
            _signer = signer;
            _invoiceNumberGenerator = invoiceNumberGenerator;
            _invoiceBucketGenerator = invoiceBucketGenerator;
            _grandTotalAuditTrailBucketGenerator = grandTotalAuditTrailBucketGenerator;
            _grandTotalAuditTrailStore = grandTotalAuditTrailStore;
        }

        public override async Task Handle(Invoice invoice)
        {
            var invoiceChainContext = new ChainContext(invoice.AccountId, _invoiceBucketGenerator.Generate(invoice));
            var grandTotalAuditTrailChainContext = new ChainContext(invoice.Supplier.Id, _grandTotalAuditTrailBucketGenerator.Generate(invoice));

            var lastInvoiceChainable = await _chainStore.Last(invoiceChainContext);
            var lastGrandTotalAuditTrailChainable = await _chainStore.Last(grandTotalAuditTrailChainContext);

            invoice.Sequence = lastInvoiceChainable.NextSequence;
            invoice.Number = await _invoiceNumberGenerator.Generate(invoice);
            invoice.Signature = await _signer.Sign(invoice, lastInvoiceChainable.Signature);

            await _invoiceStore.Store(invoice);
            var grandTotalAuditTrail = await _grandTotalAuditTrailStore.Create(invoice, lastGrandTotalAuditTrailChainable.CumulatedValue);

            await UpdateInvoiceChains(invoice, invoiceChainContext, lastInvoiceChainable,
                grandTotalAuditTrail, grandTotalAuditTrailChainContext, lastGrandTotalAuditTrailChainable);
            
            await base.Handle(invoice);
        }

        private async Task UpdateInvoiceChains(Invoice invoice, ChainContext invoiceChainContext, IChainable lastInvoiceChainable,
            GrandTotalAuditTrail grandTotalAuditTrail, ChainContext grandTotalAuditTrailChainContext, IChainable lastGrandTotalAuditTrailChainable)
        {
            try
            {
                await lastInvoiceChainable.Append(invoice.Signature);
                try
                {
                    await lastGrandTotalAuditTrailChainable.Append(await _signer.Sign(invoice, lastGrandTotalAuditTrailChainable.Signature), grandTotalAuditTrail.GrandTotal);
                }
                catch (Exception chainException)
                {
                    // Failed to append to the chain, means we have to
                    // rollback the grandTotalAuditTrail because it's not stored in a chain
                    _logger.LogError(chainException,
                        $"failed to append to chain {grandTotalAuditTrailChainContext}, deleting created grandTotalAuditTrail {grandTotalAuditTrail.Id}");

                    try
                    {
                        await _grandTotalAuditTrailStore.Delete(grandTotalAuditTrail);
                        _logger.LogInformation("successfully removed grandTotalAuditTrail, chain has been restored");
                    }
                    catch (Exception grandTotalAuditException)
                    {
                        _logger.LogError(grandTotalAuditException,
                            "failed to delete grandTotalAuditTrail after appending to the chain failed");
                    }

                    // Throw the exception again in order to remove the Invoice as well
                    // TODO: Check if this is expected behaviour
                    throw new Exception(chainException.Message);
                }
            }
            catch (Exception chainException)
            {
                // Failed to append to the chain, means we have to
                // rollback the invoice because it's not stored in a chain
                _logger.LogError(chainException,
                    $"failed to append to chain {invoiceChainContext}, deleting created invoice {invoice.Id}");

                try
                {
                    await _invoiceStore.Delete(invoice);
                    _logger.LogInformation("successfully removed invoice, chain has been restored");
                }
                catch (Exception invoiceException)
                {
                    // TODO(kevin): this means the chain is now in an invalid state
                    // ^ how do we recover from this?
                    _logger.LogError(invoiceException,
                        "failed to delete invoice after appending to the chain failed");
                }
            }
        }
    }
}
