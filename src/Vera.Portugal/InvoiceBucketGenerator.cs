using System.Text;
using Vera.Models;

namespace Vera.Portugal
{
    public sealed class InvoiceBucketGenerator : IInvoiceBucketGenerator
    {
        public string Generate(Invoice invoice)
        {
            var sb = new StringBuilder(invoice.StoreNumber)
                .Append("-")
                .Append(InvoiceTypeHelper.DetermineType(invoice));

            if (invoice.Manual)
            {
                sb.Append("-m");
            }

            // format: {StoreNumber}-{Type}[-m]
            return sb.ToString();
        }
    }
}