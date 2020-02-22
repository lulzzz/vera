using Vera.Models;

namespace Vera
{
    public sealed class UniqueSequencePerStoreGenerator : IInvoiceSequenceGenerator
    {
        public string Generate(Invoice invoice) => invoice.StoreNumber;
    }
}