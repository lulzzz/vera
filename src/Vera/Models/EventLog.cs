using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
