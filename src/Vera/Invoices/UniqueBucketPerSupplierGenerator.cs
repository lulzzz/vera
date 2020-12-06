using Vera.Models;

namespace Vera.Invoices
{
    public sealed class UniqueBucketPerSupplierGenerator : IInvoiceBucketGenerator
    {
        public string Generate(Invoice invoice) => invoice.Supplier.SystemId;
    }
}