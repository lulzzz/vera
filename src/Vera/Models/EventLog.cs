using System;
using System.Collections.Generic;

namespace Vera.Models
{
    public class EventLog
    {
        public EventLog()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        public EventLogType Type { get; set; }

        public string? RegisterId { get; set; }
            
        /// <summary>
        /// Supplier that triggered the event log.
        /// </summary>
        public Supplier Supplier { get; set; }

        /// <summary>
        /// Date that the event log was created.
        /// </summary>
        public DateTime Date { get; set; }

        public string ReportNumber { get; set; }

        public string EmployeeId { get; set; }

        public IDictionary<string, string> Data { get; set; }
    }
}
