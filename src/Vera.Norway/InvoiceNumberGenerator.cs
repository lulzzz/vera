using System.Threading.Tasks;
using Vera.Invoices;
using Vera.Models;

namespace Vera.Norway
{
    public sealed class InvoiceNumberGenerator : IInvoiceNumberGenerator
    {
        public Task<string> Generate(Invoice invoice)
        {
            return Task.FromResult(invoice.Sequence.ToString());
        }
    }
}
