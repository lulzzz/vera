using System;
using System.Threading.Tasks;
using Vera.Concurrency;
using Vera.Models;
using Vera.Signing;
using Vera.Stores;

namespace Vera.Invoices
{
    public class InvoiceResult
    {
        /// <summary>
        /// Sequence number of the invoice.
        /// </summary>
        public int Sequence { get; set; }

        /// <summary>
        /// Generated invoice number.
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Input that was used to generate the <see cref="Output"/>.
        /// </summary>
        public string Input { get; set; }

        /// <summary>
        /// Resulting signature.
        /// </summary>
        public byte[] Output { get; set; }
    }

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
        Task Process(Invoice invoice);
    }

    public sealed class InvoiceProcessor : IInvoiceProcessor
    {
        private readonly IInvoiceStore _store;
        private readonly ILocker _locker;
        private readonly IComponentFactory _factory;
        private readonly InvoiceTotalsCalculator _invoiceTotalsCalculator;

        public InvoiceProcessor(IInvoiceStore store, ILocker locker, IComponentFactory factory)
        {
            _store = store;
            _locker = locker;
            _factory = factory;
            _invoiceTotalsCalculator = new InvoiceTotalsCalculator();
        }

        public async Task Process(Invoice invoice)
        {
            var invoiceBucketGenerator = _factory.CreateInvoiceBucketGenerator();

            var bucket = invoiceBucketGenerator.Generate(invoice);

            // Lock on the unique sequence of the invoice so no other invoice can enter
            // the bucket to ensure the sequence stays in-order
            await using (await _locker.Lock(bucket, TimeSpan.FromSeconds(15)))
            {
                var invoiceNumberGenerator = _factory.CreateInvoiceNumberGenerator();
                var packageSigner = _factory.CreatePackageSigner();

                // Get last stored invoice based on the bucket for the invoice
                var last = await _store.Last(invoice.AccountId, bucket);

                invoice.Totals = _invoiceTotalsCalculator.Calculate(invoice);
                invoice.Sequence = last?.Sequence + 1 ?? 1;
                invoice.Number = await invoiceNumberGenerator.Generate(invoice);
                invoice.Signature = await packageSigner.Sign(new Package(invoice, last));

                await _store.Store(invoice, bucket);
            }
        }
    }
}