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

        /// <summary>
        /// Register for which this event was logged. Can be null because not all events require a register.
        /// </summary>
        public Guid? RegisterId { get; set; }

        /// <summary>
        /// <see cref="RegisterId"/>
        /// </summary>
        public string? RegisterSystemId { get; set; }

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
