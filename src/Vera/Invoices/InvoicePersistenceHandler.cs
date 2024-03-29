﻿using System;
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

        public InvoicePersistenceHandler(
            ILogger<InvoicePersistenceHandler> logger,
            IChainStore chainStore,
            IInvoiceStore invoiceStore, 
            IInvoiceSigner signer,
            IInvoiceNumberGenerator invoiceNumberGenerator,
            IBucketGenerator<Invoice> invoiceBucketGenerator
        )
        {
            _logger = logger;
            _chainStore = chainStore;
            _invoiceStore = invoiceStore;
            _signer = signer;
            _invoiceNumberGenerator = invoiceNumberGenerator;
            _invoiceBucketGenerator = invoiceBucketGenerator;
        }

        public override async Task Handle(Invoice invoice)
        {
            var bucket = _invoiceBucketGenerator.Generate(invoice);
            var chainContext = new ChainContext(invoice.AccountId, bucket);
            
            // Get last stored invoice based on the bucket for the invoice
            var last = await _chainStore.Last(chainContext);

            invoice.Sequence = last.NextSequence;
            invoice.Number = await _invoiceNumberGenerator.Generate(invoice);
            invoice.Signature = await _signer.Sign(invoice, last.Signature);

            await _invoiceStore.Store(invoice);

            try
            {
                await last.Append(invoice.Signature);
            }
            catch (Exception chainException)
            {
                // Failed to append to the chain, means we have to
                // rollback the invoice because it's not stored in a chain
                _logger.LogError(chainException,
                    $"failed to append to chain {chainContext}, deleting created invoice {invoice.Id}");

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
            
            await base.Handle(invoice);
        }
    }
}