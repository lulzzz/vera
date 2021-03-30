using System.Threading.Tasks;
using Vera.Invoices;
using Vera.Models;

namespace Vera.Portugal.Invoices
{
    public sealed class InvoiceNumberGenerator : IInvoiceNumberGenerator
    {
        public Task<string> Generate(Invoice invoice)
        {
            var type = InvoiceTypeHelper.DetermineType(invoice);

            var subtype = $"{(invoice.Manual ? "m" : "it")}{type}";
            var series = $"{invoice.Supplier.SystemId}{(invoice.Manual ? "m" : string.Empty)}";

            return Task.FromResult($"{subtype} {series}/{invoice.Sequence}");
        }
    }
}