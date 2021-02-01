using System;

namespace Vera.Audits
{
    public sealed class AuditCriteria
    {
        public Guid AccountId { get; set; }
        
        // TODO(kevin): refers to the number of the store from the 3th party
        public string SupplierSystemId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}