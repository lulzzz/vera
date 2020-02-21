using System.Threading.Tasks;
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
        private readonly IInvoiceLocker _locker;
        private readonly IInvoiceSequenceGenerator _invoiceSequenceGenerator;
        private readonly IInvoiceNumberGenerator _invoiceNumberGenerator;
        private readonly IPackageSigner _packageSigner;

        public InvoiceFacade(
            IInvoiceStore store,
            IInvoiceLocker locker,
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
            var sequence = _invoiceSequenceGenerator.Generate(invoice);

            // Lock on the unique sequence of the invoice
            using (await _locker.Lock(sequence))
            {
                // Get last stored invoice based on the unique sequence for the invoice
                var last = await _store.Last(sequence);

                // Generate number for this invoice
                var number = await _invoiceNumberGenerator.Generate(invoice, last);

                var result = await _packageSigner.Sign(new Package(invoice, last)
                {
                    Number = number
                });

                invoice.RawSignature = result.Input;
                invoice.Signature = result.Output;

                await _store.Save(invoice);
            }
        }
    }
}