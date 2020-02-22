using System;
using System.Threading.Tasks;
using Vera.Concurrency;
using Vera.Models;
using Vera.Signing;
using Vera.Stores;

namespace Vera
{
    public interface IInvoiceFacade
    {
        Task Process(Invoice invoice);
    }

    public sealed class InvoiceFacade : IInvoiceFacade
    {
        private readonly IInvoiceStore _store;
        private readonly ILocker _locker;
        private readonly IInvoiceSequenceGenerator _invoiceSequenceGenerator;
        private readonly IInvoiceNumberGenerator _invoiceNumberGenerator;
        private readonly IPackageSigner _packageSigner;

        public InvoiceFacade(
            IInvoiceStore store,
            ILocker locker,
            IInvoiceSequenceGenerator invoiceSequenceGenerator,
            IInvoiceNumberGenerator invoiceNumberGenerator,
            IPackageSigner packageSigner
        )
        {
            _store = store;
            _locker = locker;
            _invoiceSequenceGenerator = invoiceSequenceGenerator;
            _invoiceNumberGenerator = invoiceNumberGenerator;
            _packageSigner = packageSigner;
        }

        public async Task Process(Invoice invoice)
        {
            // TODO(kevin): needed?
            var clone = new Invoice(invoice);

            var sequence = _invoiceSequenceGenerator.Generate(clone);

            // Lock on the unique sequence of the invoice
            await using (await _locker.Lock(sequence, TimeSpan.FromMinutes(1)))
            {
                // Get last stored invoice based on the unique sequence for the invoice
                var last = await _store.Last(sequence);

                clone.Sequence = last.Sequence + 1;

                // Generate number for this invoice
                var number = await _invoiceNumberGenerator.Generate(clone, last);

                var result = await _packageSigner.Sign(new Package(clone, last)
                {
                    Number = number
                });

                clone.RawSignature = result.Input;
                clone.Signature = result.Output;

                await _store.Save(clone);
            }
        }
    }
}