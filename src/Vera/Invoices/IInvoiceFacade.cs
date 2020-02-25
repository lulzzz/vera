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
        /// Input that was used to generate the <see cref="Signature"/>.
        /// </summary>
        public string RawSignature { get; set; }

        /// <summary>
        /// Resulting signature.
        /// </summary>
        public byte[] Signature { get; set; }
    }

    public interface IInvoiceFacade
    {
        Task<InvoiceResult> Process(Invoice invoice);
    }

    public sealed class InvoiceFacade : IInvoiceFacade
    {
        private readonly IInvoiceStore _store;
        private readonly ILocker _locker;
        private readonly IInvoiceBucketGenerator _invoiceBucketGenerator;
        private readonly IInvoiceNumberGenerator _invoiceNumberGenerator;
        private readonly IPackageSigner _packageSigner;

        public InvoiceFacade(
            IInvoiceStore store,
            ILocker locker,
            IInvoiceBucketGenerator invoiceBucketGenerator,
            IInvoiceNumberGenerator invoiceNumberGenerator,
            IPackageSigner packageSigner
        )
        {
            _store = store;
            _locker = locker;
            _invoiceBucketGenerator = invoiceBucketGenerator;
            _invoiceNumberGenerator = invoiceNumberGenerator;
            _packageSigner = packageSigner;
        }

        public async Task<InvoiceResult> Process(Invoice invoice)
        {
            var bucket = _invoiceBucketGenerator.Generate(invoice);

            // Lock on the unique sequence of the invoice
            await using (await _locker.Lock(bucket, TimeSpan.FromMinutes(1)))
            {
                // Get last stored invoice based on the bucket for the invoice
                var last = await _store.Last(invoice, bucket);

                invoice.Sequence = last?.Sequence + 1 ?? 1;

                // Generate number for this invoice
                var number = await _invoiceNumberGenerator.Generate(invoice, last);

                var result = await _packageSigner.Sign(new Package(invoice, last)
                {
                    Number = number
                });

                invoice.RawSignature = result.Input;
                invoice.Signature = result.Output;

                await _store.Save(invoice, bucket);

                return new InvoiceResult
                {
                    Sequence = invoice.Sequence,
                    Number = number,
                    RawSignature = result.Input,
                    Signature = result.Output
                };
            }
        }
    }
}