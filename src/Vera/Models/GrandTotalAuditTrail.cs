using System;

namespace Vera.Models
{
    public class GrandTotalAuditTrail
    {
        public Guid Id { get; set; }
        public Guid SupplierId { get; set; }
        public Guid InvoiceId { get; set; }
        public decimal GrandTotal { get; set; }
    }
}
