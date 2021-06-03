using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Vera.Dependencies.Handlers;
using Vera.Extensions;
using Vera.Models;
using Vera.Portugal.Stores;
using Vera.Signing;
using Vera.Stores;

namespace Vera.Portugal.WorkingDocuments
{
    public class WorkingDocumentsHandler : HandlerChain<Invoice>
    {
        private readonly IWorkingDocumentStore _wdStore;
        private readonly IChainStore _chainStore;
        private readonly IInvoiceSigner _signer;
        private readonly ILogger<WorkingDocumentsHandler> _logger;

        public WorkingDocumentsHandler(
            IWorkingDocumentStore wdStore, 
            IChainStore chainStore,
            IInvoiceSigner signer, 
            ILogger<WorkingDocumentsHandler> logger)
        {
            _wdStore = wdStore;
            _chainStore = chainStore;
            _signer = signer;
            _logger = logger;
        }

        public override async Task Handle(Invoice invoice)
        {
            var giftCardLines = invoice.Lines.Where(l => l.Product != null && l.Product.Type == ProductType.GiftCard);
            if (!giftCardLines.Any())
            {
                await base.Handle(invoice);
                return;
            }

            var wd = new Models.WorkingDocument
            {
                InvoiceId = invoice.Id,
                PeriodId = invoice.PeriodId,
                SupplierSystemId = invoice.Supplier.SystemId,
                CreationTime = invoice.Date,
            };

            wd.Lines.AddRange(giftCardLines);

            var chainContext = new ChainContext(invoice.AccountId, bucket: wd.SupplierSystemId);
            var last = await _chainStore.Last(chainContext);

            wd.Sequence = last.NextSequence;
            wd.Number = $"{Models.WorkType.CM} {wd.SupplierSystemId}/{wd.Sequence}";

            var wdInvoice = new Invoice
            {
                Number = wd.Number,
                Date = wd.CreationTime,
                Totals = new Totals { Gross = wd.Lines.Sum(l => l.Gross) }
            };
            wd.Signature = await _signer.Sign(wdInvoice, last.Signature);

            await _wdStore.Store(wd);

            try
            {
                await last.Append(wd.Signature);
            }
            catch (Exception chainException)
            {
                // Failed to append to the chain, means we have to
                // rollback the working document because it's not stored in a chain
                _logger.LogError(chainException,
                    $"failed to append to chain {chainContext}, deleting created working document {wd.Id}");

                try
                {
                    await _wdStore.Delete(wd);
                    _logger.LogInformation("successfully removed working document, chain has been restored");
                }
                catch (Exception deleteEx)
                {
                    // this means the chain is now in an invalid state
                    // ^ how do we recover from this?
                    _logger.LogError(deleteEx,
                        "failed to delete working document after appending to the chain failed");
                }
            }

            await base.Handle(invoice);
        }
    }
}
