using Vera.Models;

namespace Vera.Invoices
{
    public sealed class UniqueBucketPerStoreGenerator : IInvoiceBucketGenerator
    {
        public string Generate(Invoice invoice) => invoice.Supplier.SystemID;
    }
}