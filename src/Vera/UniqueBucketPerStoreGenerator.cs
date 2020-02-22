using Vera.Models;

namespace Vera
{
    public sealed class UniqueBucketPerStoreGenerator : IInvoiceBucketGenerator
    {
        public string Generate(Invoice invoice) => invoice.StoreNumber;
    }
}