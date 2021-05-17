using System;
using Vera.Models;

namespace Vera.EventLogs
{
    public class EventLogCriteria
    {
        public Guid AccountId { get; set; }

        public string SupplierSystemId { get; set; }

        public DateTime? StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }

        public string? RegisterId { get; set; }

        public EventLogType? Type { get; set; }
    }
}
