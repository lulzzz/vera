using Vera.Dependencies;
using Vera.Models;

namespace Vera.Invoices
{
    public sealed class UniqueBucketPerSupplierGenerator : IBucketGenerator<Invoice>
    {
        public string Generate(Invoice invoice) => invoice.Supplier.SystemId;
    }
}