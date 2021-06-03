using System;
using Vera.EventLogs;
using Vera.Grpc;

namespace Vera.Host.Mapping
{
    public static class EventLogListExtensions
    {
        public static EventLogCriteria BuildListCriteria(this ListEventLogRequest request,
            Guid accountId,
            Guid supplierId)
        {
            var unpackedType = request.Type.Unpack();

            Vera.Models.EventLogType? type = unpackedType == Vera.Models.EventLogType.None ? null : unpackedType;

            var criteria = new EventLogCriteria
            {
                EndDate = request.EndDate?.ToDateTime(),
                StartDate = request.StartDate?.ToDateTime(),
                Type = type,
                RegisterId = request.RegisterId,
                AccountId = accountId,
                SupplierId = supplierId
            };

            return criteria;
        }
    }
}