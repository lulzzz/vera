using Vera.Dependencies;
using Vera.Models;

namespace Vera.Invoices
{
    public class GrandTotalAuditTrailBucketGenerator : IBucketGenerator<Invoice>
    {
        public string Generate(Invoice entity) => $"{entity.Supplier.Id}-{nameof(Invoice)}";
    }
}
