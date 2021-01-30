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
        Task<InvoiceResult> Process(Invoice invoice);
    }

    public sealed class InvoiceProcessor : IInvoiceProcessor
    {
        private readonly IInvoiceStore _store;
        private readonly IComponentFactory _factory;

        public InvoiceProcessor(IInvoiceStore store, IComponentFactory factory)
        {
            _store = store;
            _factory = factory;
        }

        public async Task<InvoiceResult> Process(Invoice invoice)
        {
            var invoiceBucketGenerator = _factory.CreateInvoiceBucketGenerator();
            var locker = _factory.CreateLocker();

            // Prefix with accountId to make sure the bucket is unique per account
            var bucket = invoiceBucketGenerator.Generate(invoice);

            // Lock on the unique sequence of the invoice
            await using (await locker.Lock(bucket, TimeSpan.FromMinutes(1)))
            {
                var invoiceNumberGenerator = _factory.CreateInvoiceNumberGenerator();
                var packageSigner = _factory.CreatePackageSigner();

                // Get last stored invoice based on the bucket for the invoice
                var last = await _store.Last(invoice.AccountId, bucket);

                invoice.Sequence = last?.Sequence + 1 ?? 1;

                // Generate number for this invoice
                var number = await invoiceNumberGenerator.Generate(invoice);

                invoice.Number = number;

                var result = await packageSigner.Sign(new Package(invoice, last)
                {
                    Number = number
                });

                invoice.Signature = new Signature
                {
                    Input = result.Input,
                    Output = result.Output,

                    // TODO(kevin): map/get the version of the certificate
                };

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