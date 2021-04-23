using System.Collections.Generic;
using System.Threading.Tasks;
using Vera.EventLogs;
using Vera.Models;

namespace Vera.Stores
{
    public interface IEventLogStore
    {
        /// <summary>
        /// Persists the event log
        /// </summary>
        Task Store(EventLog eventLog);

        /// <summary>
        /// Lists the event logs
        /// </summary>
        Task<ICollection<EventLog>> List(EventLogCriteria eventLogCriteria);
    }
}