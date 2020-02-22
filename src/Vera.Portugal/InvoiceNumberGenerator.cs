using System.Threading.Tasks;
using Vera.Models;

namespace Vera.Portugal
{
    public sealed class InvoiceNumberGenerator : IInvoiceNumberGenerator
    {
        public Task<string> Generate(Invoice invoice, Invoice last)
        {
            var type = DetermineTypeCode(invoice);

            var subtype = $"{(invoice.Manual ? "m" : "it")}{type}";
            var series = $"{invoice.StoreNumber}{(invoice.Manual ? "m" : string.Empty)}";

            return Task.FromResult($"{subtype} {series}/{invoice.Sequence}");
        }

        private static int DetermineTypeCode(Invoice invoice)
        {
            // if total lines == total payments > receipt
            // if returns > credit
            // else return > default

            // // Invoice
            // invoice.BackendTypeID = (int)InvoiceType.FT;
            //
            // if (order.IsPaid)
            // {
            //     // Invoice receipt
            //     invoice.BackendTypeID = (int)InvoiceType.FR;
            // }
            //
            // if (order.HasReturns)
            // {
            //     // Credit note
            //     invoice.BackendTypeID = (int)InvoiceType.NC;
            // }

            return 0;
        }
    }
}