using System.Text;
using Vera.Dependencies;
using Vera.Models;

namespace Vera.Norway
{
    public sealed class InvoiceBucketGenerator : IBucketGenerator<Invoice>
    {
        public string Generate(Invoice invoice)
        {
            var sb = new StringBuilder(invoice.Supplier.SystemId);

            // format: {StoreNumber}
            return sb.ToString();
        }
    }
}
