using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.WellKnownTypes;
using Vera.Grpc;
using EventLog = Vera.Grpc.EventLog;

namespace Vera.Host.Mapping
{
    public static class EventLogExtensions
    {
        public static Vera.Models.EventLog Unpack(this EventLog eventLog)
        {
            var result = new Vera.Models.EventLog
            {
                Date = eventLog.Timestamp.ToDateTime(),
                RegisterId = eventLog.RegisterId,
                Type = eventLog.Type.Unpack()
            };

            return result;
        }

        public static IEnumerable<EventLogItem> Pack(this IEnumerable<Vera.Models.EventLog> eventLogs)
            => eventLogs.Select(Pack);

        public static Grpc.EventLogItem Pack(this Vera.Models.EventLog eventLog)
        {
            var result = new EventLogItem
            {
                Id = eventLog.Id.ToString(),
                RegisterId = eventLog.RegisterId.ToString(),
                Type = eventLog.Type.Pack(),
                SupplierSystemId = eventLog.Supplier.SystemId,
                Timestamp = Timestamp.FromDateTime(eventLog.Date)
            };

            return result;
        }
    }
}