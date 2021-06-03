using System.Threading.Tasks;
using Vera.Models;

namespace Vera.Invoices
{
    public class InvoiceSequenceNumberGenerator : IInvoiceNumberGenerator
    {
        public Task<string> Generate(Invoice invoice)
        {
            return Task.FromResult(invoice.Sequence.ToString());
        }
    }
}
