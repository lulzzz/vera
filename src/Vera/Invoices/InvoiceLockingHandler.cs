using System;
using System.Threading.Tasks;
using Vera.Concurrency;
using Vera.Models;

namespace Vera.Invoices
{
    public class InvoiceLockingHandler : InvoiceHandler
    {
        private readonly IInvoiceHandler _handler;
        private readonly IInvoiceBucketGenerator _invoiceBucketGenerator;
        private readonly ILocker _locker;

        public InvoiceLockingHandler(IInvoiceHandler handler,
            IInvoiceBucketGenerator invoiceBucketGenerator,
            ILocker locker
        )
        {
            _handler = handler;
            _invoiceBucketGenerator = invoiceBucketGenerator;
            _locker = locker;
        }

        public override async Task Handle(Invoice invoice)
        {
            var bucket = _invoiceBucketGenerator.Generate(invoice);
            
            // Lock on the unique sequence of the invoice so no other invoice can enter
            // the bucket to ensure the sequence stays in-order
            await using (await _locker.Lock(bucket, TimeSpan.FromSeconds(15)))
            {
                // Delegate to inner handler to handle something that needs to happen within the lock
                await _handler.Handle(invoice);
            }

            // Continue with the outer handler(s)
            await base.Handle(invoice);
        }
    }
}