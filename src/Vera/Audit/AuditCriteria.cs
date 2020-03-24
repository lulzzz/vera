using System;

namespace Vera.Audit
{
    public sealed class AuditCriteria
    {
        // TODO(kevin): refers to the number of the store from the 3th party
        public string SupplierSystemId { get; set; }
        
        public int StartFiscalYear { get; set; }
        public int StartFiscalPeriod { get; set; }
     
        public int EndFiscalYear { get; set; }
        public int EndFiscalPeriod { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}